// StringToIntConverter.cs

using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace HHSAdvAvalonia.Converters
{
    public class StringToIntConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // ★★★ 修正点: value を string? にキャストし、nullチェックと型チェックを分離 ★★★
            string? stringValue = value as string;

            // value が null ではなく、かつ string 型であるかを確認
            if (stringValue != null)
            {
                // targetType が int またはそれに相当する型である必要はありませんが、
                // 変換が目的であるため、Int32.TryParse を呼び出す
                if (int.TryParse(stringValue, out int result))
                {
                    // 変換成功: int 型の結果を返す
                    return result;
                }
            }
            
            // 変換に失敗した場合、または値が string でなかった場合は、元の値を返す（バインディングエラーを回避するため）
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}