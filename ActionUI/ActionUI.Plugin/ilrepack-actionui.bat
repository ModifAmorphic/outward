
dotnet publish ModifAmorphic.Outward.ActionUI.Plugin.csproj --configuration Debug
cd D:\repos\modifamorphic\azure\unpacked-shared-assemblies\
ILRepack /internalize /wildcards D:\repos\Github-Outward\actionui\outward\ActionUI\ActionUI.Plugin\bin\Debug\netstandard2.0\publish\ModifAmorphic.Outward.ActionUI.dll D:\repos\Github-Outward\actionui\outward\ActionUI\ActionUI.Plugin\bin\Debug\netstandard2.0\publish\ModifAmorphic.Outward.ActionUI.Plugin.dll Newtonsoft.Json.dll D:\repos\Github-Outward\actionui\outward\ActionUI\ActionUI.Plugin\bin\Debug\netstandard2.0\publish\ModifAmorphic.Outward.Shared.*.dll /out:D:\repos\Github-Outward\actionui\outward\ActionUI\ActionUI.Plugin\bin\Debug\netstandard2.0\publish\ilrepack\ModifAmorphic.Outward.ActionUI.dll
cd D:\repos\Github-Outward\actionui\outward\ActionUI\ActionUI.Plugin
copy bin\Debug\netstandard2.0\publish\ilrepack\ModifAmorphic.Outward.ActionUI.dll C:\Users\Justin\AppData\Roaming\r2modmanPlus-local\OutwardDe\profiles\Debugging\BepInEx\plugins\ModifAmorphic-ActionUI\ModifAmorphic.Outward.ActionUI.dll /Y
