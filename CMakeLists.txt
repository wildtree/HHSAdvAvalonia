cmake_minimum_required(VERSION 3.20)
project(HHSAdvAvalonia NONE)  # C# プロジェクトは外部で管理するので NONE

# バージョンを csproj から取得
execute_process(
  COMMAND dotnet msbuild HHSAdvAvalonia.csproj -nologo -property:TargetFramework=net8.0 -getproperty:Version
  OUTPUT_VARIABLE APP_VERSION
  OUTPUT_STRIP_TRAILING_WHITESPACE
)

message(STATUS "App version: ${APP_VERSION}")

set(PUBLISH_DIR ${CMAKE_BINARY_DIR}/publish)
# dotnet publish タスク
add_custom_target(publish ALL
  COMMAND dotnet publish -c Release -r linux-x64 --self-contained false
  	-o ${PUBLISH_DIR}
  WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
)
set(PACKAGE_DIR ${CMAKE_BINARY_DIR}/package)

add_custom_target(stage ALL
  COMMAND ${CMAKE_COMMAND} -E make_directory ${PACKAGE_DIR}/opt/hhsadvavalonia
  COMMAND ${CMAKE_COMMAND} -E copy_directory ${PUBLISH_DIR} ${PACKAGE_DIR}/opt/hhsadvavalonia
  DEPENDS publish
)

# control ファイルを自動生成
file(WRITE ${CMAKE_BINARY_DIR}/control
"Package: hhsadvavalonia
Version: ${APP_VERSION}
Section: utils
Priority: optional
Architecture: amd64
Maintainer: Hiroyuki Araki <wildtree@gmail.com>
Depends: vlc (>= 3.0), libvlc-bin (>= 3.0), libvlc5 (>= 3.0)
Description: High High School Adventure for AvaloniaUI
 An implementation of 'High High School Adventure' with AvaloniaUI.
")

# .desktop ファイルを自動生成
file(WRITE ${CMAKE_BINARY_DIR}/hhsadvavalonia.desktop
"[Desktop Entry]
Name=High High School Adventure AvaloniaUI
Comment=High High School Adventure AvaloniaUI v${APP_VERSION}
Exec=HHSAdvAvalonia
Icon=hhsadvavalonia
Terminal=false
Type=Application
Categories=Game;AdventureGame
")

# exec file
file(WRITE ${CMAKE_BINARY_DIR}/HHSAdvAvalonia
"#!/usr/bin/sh
exec /opt/hhsadvavalonia/HHSAdvAvalonia \"$@\"
")
# 実行権限を付与
file(CHMOD ${CMAKE_BINARY_DIR}/HHSAdvAvalonia
     PERMISSIONS OWNER_READ OWNER_WRITE OWNER_EXECUTE
                 GROUP_READ GROUP_EXECUTE
                 WORLD_READ WORLD_EXECUTE
)


# DEB パッケージ生成
add_custom_target(deb ALL
  COMMAND ${CMAKE_COMMAND} -E make_directory ${PACKAGE_DIR}/DEBIAN
  COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_BINARY_DIR}/control ${PACKAGE_DIR}/DEBIAN/control
  COMMAND ${CMAKE_COMMAND} -E make_directory ${PACKAGE_DIR}/usr/share/applications
  COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_BINARY_DIR}/hhsadvavalonia.desktop ${PACKAGE_DIR}/usr/share/applications/
  COMMAND ${CMAKE_COMMAND} -E make_directory ${PACKAGE_DIR}/usr/bin
  COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_BINARY_DIR}/HHSAdvAvalonia ${PACKAGE_DIR}/usr/bin/
  COMMAND ${CMAKE_COMMAND} -E make_directory ${PACKAGE_DIR}/usr/share/icons/hicolor/48x48/apps/
  COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_SOURCE_DIR}/data/icon.png ${PACKAGE_DIR}/usr/share/icons/hicolor/48x48/apps/hhsadvavalonia.png
  COMMAND dpkg-deb --build ${PACKAGE_DIR} hhsadvavalonia_${APP_VERSION}_amd64.deb
  DEPENDS stage
)
