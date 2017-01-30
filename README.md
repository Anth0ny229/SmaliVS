### About SmaliVS

SmaliVS is a Visual Studio project and language extension for the smali file type. Smali is the disassembly output from APK's using the smali\baksmali tools.

Using this extension will allow you to dump an apk form your device. It will automatally dissasemble the apk and create a Visual Studio Project. From there you can modify the .smali files from within Visual Studio, then use Visual Studios Build\Deploy to reinstall the modified apk. This extension uses ApkTool as well as smali\baksmali to do the important work with the apks and is already packaged with the extension. Java is a requirement and java should be added to your environment variables.

#### Features
- Apk dumping from connected device
- Automatic disassembly into a Visual Studio smali project
- Project Settings
- Syntax\Keyword highlighting
- Basic autocompletion of opcodes
- Quick Info of opcodes function
- Rebuilding of the smali project
- Deployment to connected device

#### Known Issues
- Quick Info for opcodes that include a '\' or '-' won't display properly
- No debugging support (Don't plan on adding it at the momment)

#### TODO
- Fix Quick Info issue
- Clean up Project Wizard GUI
- Add better error handling and checking

#### Some useful links for getting started with smali
- [Official dex bytecode reference](https://source.android.com/devices/tech/dalvik/dalvik-bytecode.html)
- [Registers wiki page](https://github.com/JesusFreke/smali/wiki/Registers)
- [Types, Methods and Fields wiki page](https://github.com/JesusFreke/smali/wiki/TypesMethodsAndFields)
- [Official dex format reference](https://source.android.com/devices/tech/dalvik/dex-format.html)

#### Special Thanks
- JesusFreke (smali\baksmali)
- iBotPeaches (ApkTool)
