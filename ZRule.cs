// Hign High School Adventure SDL -- Game Rule Engine

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace HHSAdvAvalonia
{
    public class ZCore
    {
        public class ZCommand
        {
            public enum Command { Nop, Message, Sound, Dialog, GameOver }
            public Command Cmd { get; private set; }
            public byte Operand { get; private set; }
            public ZCommand() { Cmd = Command.Nop; Operand = 0; }
            public ZCommand(Command c, byte o)
            {
                Cmd = c; Operand = o;
            }
        }
        public class ZStatus
        {
            public byte[] raw = new byte[8]; // 8 bytes
            public byte MapId { get { return raw[0]; } set { raw[0] = value; } }
            public byte MapViewId { get { return raw[1]; } set { raw[1] = value; } }
            public byte CmdId { get { return raw[2]; } set { raw[2] = value; } }
            public byte ObjId { get { return raw[3]; } set { raw[3] = value; } }
            public byte DialogResult { get { return raw[4]; } set { raw[4] = value; } }
            public byte Random { get { return raw[5]; } set { raw[5] = value; } }
            public byte DialogOk { get { return raw[6]; } set { raw[6] = value; } }
            public byte DialogMessage { get { return raw[7]; } set { raw[7] = value; } }
            public ZStatus()
            {
                raw = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            }
        }
        private ZStatus status = new ZStatus();
        private Random rand = new Random();
        private Queue<ZCommand> commandQueue = new Queue<ZCommand>();

        private static ZCore? instance = null;
        public static ZCore Instance
        {
            get
            {
                if (instance == null) instance = new ZCore();
                return instance;
            }
        }

        private ZCore()
        {
            status.MapId = 0;
            status.MapViewId = 0;
            status.CmdId = 0;
            status.ObjId = 0;
            status.DialogResult = 0;
            status.Random = (byte)rand.Next(256);
            status.DialogOk = 0;
            status.DialogMessage = 0;
        }
        public byte MapId { get { return status.MapId; } set { status.MapId = value; } }
        public byte MapViewId { get { return status.MapViewId; } set { status.MapViewId = value; } }
        public byte CmdId { get { return status.CmdId; } set { status.CmdId = value; } }
        public byte ObjId { get { return status.ObjId; } set { status.ObjId = value; } }
        public byte DlgRes { get { return status.DialogResult; } set { status.DialogResult = value; } }

        public void setValue(int id, int v)
        {
            switch (id)
            {
                case 0: status.MapId = (byte)(v & 0xff); break;
                case 1: status.MapViewId = (byte)(v & 0xff); break;
                case 2: status.CmdId = (byte)(v & 0xff); break;
                case 3: status.ObjId = (byte)(v & 0xff); break;
                case 4: status.DialogResult = (byte)(v & 0xff); break;
                case 5: status.Random = (byte)(v & 0xff); rand = new Random(status.Random); break;
                case 6: status.DialogOk = (byte)(v & 0xff); break;
                case 7: status.DialogMessage = (byte)(v & 0xff); break;
            }
        }
        public int getValue(int id)
        {
            switch (id)
            {
                case 0: return status.MapId;
                case 1: return status.MapViewId;
                case 2: return status.CmdId;
                case 3: return status.ObjId;
                case 4: return status.DialogResult;
                case 5: status.Random = (byte)rand.Next(256); return status.Random;
                case 6: return status.DialogOk;
                case 7: return status.DialogMessage;
            }
            return 0;
        }
        public byte Random
        {
            get { status.Random = (byte)rand.Next(256); return status.Random; }
            set { rand = new Random(value); }
        }
        public ZCommand pop() => commandQueue.Count > 0 ? commandQueue.Dequeue() : new ZCommand();
        public void push(ZCommand c) => commandQueue.Enqueue(c);
        public void push(ZCommand.Command c, byte o) => commandQueue.Enqueue(new ZCommand(c, o));
        public byte[] pack() => status.raw;
        public int packedSize { get { return status.raw.Length; } }
        public void unpack(byte[] b)
        {
            if (b.Length != 8) throw new ArgumentException("ZStatus must be 8 bytes length");
            status.raw = b;
        }
    }

    public class ZRuleBlock
    {
        private byte[] rules;
        public ZRuleBlock(byte[] r) { rules = r; }
        public byte Action { get { return (byte)((rules[0] >> 7) & 1); } }
        public byte Op { get { return (byte)((rules[0] >> 4) & 7); } }
        public byte Type { get { return (byte)((rules[1] >> 5) & 7); } }
        public byte Id { get { return (byte)(rules[1] & 0x1F); } }
        public byte Offset { get { return rules[2]; } }
        public byte BodyType { get { return (byte)((rules[2] >> 5) & 7); } }
        public byte BodyId { get { return (byte)(rules[2] & 0x1F); } }
        public byte Value { get { return rules[3]; } }

        public enum ZType { Comp = 0, Act = 1 }
        public enum ZComp { Nop = 0, Eq = 1, Ne = 2, Gt = 3, Ge = 4, Lt = 5, Le = 6 }
        public enum ZAct { Nop = 0, Move = 1, Assign = 2, Message = 3, Dialog = 4, Look = 5, Sound = 6, GameOver = 7 }
        public enum ZBodyType { None = 0, Fact = 1, Place = 2, System = 3, Vector = 4 }


        private byte Op1()
        {
            byte v = 0;
            switch (Type)
            {
                case (byte)ZBodyType.Fact:
                    v = ZUserData.Instance.getFact(Id); break;
                case (byte)ZBodyType.Place:
                    v = ZUserData.Instance.getPlace(Id); break;
                case (byte)ZBodyType.System:
                    v = (byte)ZCore.Instance.getValue(Id); break;
                case (byte)ZBodyType.Vector:
                    v = ZUserData.Instance.getLink(Offset - 1).get(Id);
                    break;

            }
            return v;
        }

        private int Op2()
        {
            int v = 0;
            if (BodyType == (byte)ZBodyType.None || Type == (byte)ZBodyType.Vector) return Value;
            switch (BodyType)
            {
                case (byte)ZBodyType.Fact:
                    v = ZUserData.Instance.getFact(BodyId); break;
                case (byte)ZBodyType.Place:
                    v = ZUserData.Instance.getPlace(BodyId); break;
                case (byte)ZBodyType.System:
                    v = ZCore.Instance.getValue(BodyId); break;
            }
            return v;
        }
        public bool IsAction()
        {
            return Action == (byte)ZType.Act;
        }
        public bool Evaluate()
        {
            bool ok = false;
            switch (Action)
            {
                case (byte)ZType.Comp:
                    byte v1 = Op1();
                    int v2 = Op2();
                    switch (Op)
                    {
                        case (byte)ZComp.Nop:
                            ok = true; break;
                        case (byte)ZComp.Eq:
                            ok = v1 == v2; break;
                        case (byte)ZComp.Ne:
                            ok = v1 != v2; break;
                        case (byte)ZComp.Gt:
                            ok = v1 > v2; break;
                        case (byte)ZComp.Ge:
                            ok = v1 >= v2; break;
                        case (byte)ZComp.Lt:
                            ok = v1 < v2; break;
                        case (byte)ZComp.Le:
                            ok = v1 <= v2; break;
                        default:
                            throw new ArgumentException("Invalid Comparison");
                    }
                    break;
                case (byte)ZType.Act:
                    ZCore core = ZCore.Instance;
                    ZUserData userData = ZUserData.Instance;
                    switch (Op)
                    {
                        case (byte)ZAct.Nop:
                            ok = true; break;
                        case (byte)ZAct.Move:
                            byte v = userData.getLink(core.MapId - 1).get(Value);
                            if (v != 0)
                            {
                                core.MapId = v;
                            }
                            else
                            {
                                if (userData.getFact(1) == core.MapId && core.Random > 85)
                                {
                                    core.push(ZCore.ZCommand.Command.Message, 0xb5); // 先生につかまった
                                    core.push(ZCore.ZCommand.Command.GameOver, 1);
                                    userData.setFact(1, 0); // 学校に戻る
                                    ok = false;
                                }
                                else
                                {
                                    core.push(ZCore.ZCommand.Command.Message, 0xb6); // そこには行けません
                                    ok = true;
                                }
                            }
                            break;
                        case (byte)ZAct.Assign:
                            switch (Type)
                            {
                                case (byte)ZBodyType.Fact:
                                    userData.setFact(Id, (byte)Op2()); break;
                                case (byte)ZBodyType.Place:
                                    userData.setPlace(Id, (byte)Op2()); break;
                                case (byte)ZBodyType.System:
                                    core.setValue(Id, Op2() & 0xff);
                                    if (Id == 5) core.Random = 0;
                                    break;
                                case (byte)ZBodyType.Vector:
                                    userData.getLink(Offset - 1).set(Id, (byte)(Op2() & 0xff));
                                    break;
                                case (byte)ZBodyType.None:
                                    break;
                                default:
                                    throw new ArgumentException("Invalid Body Type");
                            }
                            ok = true; break;
                        case (byte)ZAct.Message:
                            core.push(ZCore.ZCommand.Command.Message, Value);
                            ok = true;
                            break;
                        case (byte)ZAct.Dialog:
                            core.push(ZCore.ZCommand.Command.Dialog, Value);
                            ok = true;
                            break;
                        case (byte)ZAct.Look:
                            if (Value == 0)
                            {
                                core.MapId = core.MapViewId;
                                core.MapViewId = 0;
                            }
                            else
                            {
                                core.MapViewId = core.MapId;
                                core.MapId = Value;
                            }
                            ok = true;
                            break;
                        case (byte)ZAct.Sound:
                            core.push(ZCore.ZCommand.Command.Sound, Value);
                            ok = true;
                            break;
                        case (byte)ZAct.GameOver:
                            switch (Value)
                            {
                                case 0:
                                    //userData.setFact(1, 0); // 先生はいなくなる
                                    core.push(ZCore.ZCommand.Command.Message, 0xee); // ゲームオーバー
                                    core.push(ZCore.ZCommand.Command.GameOver, 1);
                                    break;
                                case 1:
                                    core.push(ZCore.ZCommand.Command.Message, 0xee); // ゲームオーバー
                                    core.push(ZCore.ZCommand.Command.GameOver, 2);
                                    break;
                                case 2:
                                    core.push(ZCore.ZCommand.Command.Message, 0xef); // ゲームクリア
                                    core.push(ZCore.ZCommand.Command.GameOver, 3);
                                    break;
                            }
                            ok = true;
                            break;
                        default:
                            throw new ArgumentException("Invalid Action");
                    }
                    break;
            }
            return ok;
        }

    }

    public class ZRuleBase
    {
        public const int MaxRules = 256;
        public const int FileBlockSize = 96;
        public const int RuleBlockLength = FileBlockSize / 4 - 1;
        public const byte EndMarker = 0xFF;

        public byte CmdId { get; private set; }
        public byte ObjId { get; private set; }
        public byte MapId { get; private set; }

        private ZRuleBlock[] rules = new ZRuleBlock[RuleBlockLength];

        public ZRuleBase(byte[] buf)
        {
            MapId = buf[0];
            CmdId = buf[1];
            ObjId = buf[2];
            for (int i = 0; i < RuleBlockLength; i++)
            {
                byte[] r = new byte[4];
                Array.Copy(buf, 4 + i * 4, r, 0, 4);
                rules[i] = new ZRuleBlock(r);
            }

        }

        public bool IsTarget(byte map_id, byte cmd_id, byte obj_id)
        {
            return (MapId == 0 || MapId == map_id) &&
                   (CmdId == 0 || CmdId == cmd_id) &&
                   (ObjId == 0 || ObjId == obj_id);
        }

        public bool Evaluate()
        {
            ZCore c = ZCore.Instance;
            ZUserData u = ZUserData.Instance;

            if (IsTarget(c.MapId, c.CmdId, c.ObjId))
            {
                int i = 0;
                while (!rules[i].IsAction())
                {
                    if (!rules[i++].Evaluate()) return false;
                }
                while (rules[i].Op != (byte)ZRuleBlock.ZAct.Nop)
                {
                    rules[i++].Evaluate();
                }
                return true;
            }
            return false;
        }
    }

    public class ZRules
    {
        private string fileName = string.Empty;
        private const int fileBlockSize = 96;
        private ZRuleBase[] rules;

        public ZRuleBase[] Rules { get { return rules;  }}
        public ZRules(string f)
        {
            fileName = f;
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                byte[] buf = new byte[fileBlockSize];
                long n = fs.Length / fileBlockSize;
                rules = new ZRuleBase[n];
                using (var br = new BinaryReader(fs))
                {
                    for (int i = 0; i < n; i++)
                    {
                        buf = br.ReadBytes(fileBlockSize);
                        rules[i] = new ZRuleBase(buf);
                    }
                }
            }
        }
    }
}

