# OrleansExample

# 1. HelloWorld

### Add References

1. `Grains` references `GrainInterfaces`.
2. `Silo` references `GrainInterfaces` and `Grains`.
3. `Client` references `GrainInterfaces`.

## Add Orleans NuGet Packages

| Project          | Nuget Package                               |
| :--------------- | :------------------------------------------ |
| Silo             | `Microsoft.Orleans.Server`                  |
| Silo             | `Microsoft.Extensions.Logging.Console`      |
| Client           | `Microsoft.Extensions.Logging.Console`      |
| Client           | `Microsoft.Orleans.Client`                  |
| Grain Interfaces | `Microsoft.Orleans.Core.Abstractions`       |
| Grain Interfaces | `Microsoft.Orleans.CodeGenerator.MSBuild`   |
| Grains           | `Microsoft.Orleans.CodeGenerator.MSBuild`   |
| Grains           | `Microsoft.Orleans.Core.Abstractions`       |
| Grains           | `Microsoft.Extensions.Logging.Abstractions` |

# 2. Use Kcp

1. 启动Console Silo，启动Console Client（GameServer）连接Silo。
2. 启动Unity Client，连接服务器。