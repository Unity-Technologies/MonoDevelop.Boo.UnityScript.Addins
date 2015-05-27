rm -rf build-mpacks
rm -rf build

perl external/build_boo_unityscript_mac.pl

xbuild /p:OutputPath=../build /p:Configuration=Release /t:Rebuild

mkdir build-mpacks
cd build-mpacks

mono "../../monodevelop/main/build/bin/mdtool.exe" setup pack ../build/Boo.MonoDevelop.dll ../build/UnityScript.MonoDevelop.dll

cd ..