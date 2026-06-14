import { dotnet } from './_framework/dotnet.js'

const dotnetRuntime = await dotnet
    .withApplicationArguments("app")
    .create();

const config = dotnetRuntime.getConfig();
await dotnetRuntime.runMain(config.mainAssemblyName, []);
