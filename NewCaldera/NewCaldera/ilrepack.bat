
dotnet publish ModifAmorphic.Outward.NewCaldera.csproj --configuration Debug
cd D:\repos\modifamorphic\azure\unpacked-shared-assemblies\
ILRepack /internalize /wildcards D:\repos\modifamorphic\GitHub-Outward\mods\newcaldera\outward\NewCaldera\NewCaldera\bin\Debug\netstandard2.0\publish\ModifAmorphic.Outward.NewCaldera.dll Newtonsoft.Json.dll D:\repos\modifamorphic\GitHub-Outward\mods\newcaldera\outward\NewCaldera\NewCaldera\bin\Debug\netstandard2.0\publish\ModifAmorphic.Outward.Shared.*.dll /out:D:\repos\modifamorphic\GitHub-Outward\mods\newcaldera\outward\NewCaldera\NewCaldera\bin\Debug\netstandard2.0\publish\ilrepack\ModifAmorphic.Outward.NewCaldera.dll
cd D:\repos\modifamorphic\GitHub-Outward\mods\newcaldera\outward\NewCaldera\NewCaldera\
copy bin\Debug\netstandard2.0\publish\ilrepack\ModifAmorphic.Outward.NewCaldera.dll C:\Users\Justin\AppData\Roaming\r2modmanPlus-local\OutwardDe\profiles\Debugging\BepInEx\plugins\ModifAmorphic-ActionUI\ModifAmorphic.Outward.NewCaldera.dll /Y
