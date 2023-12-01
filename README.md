# LockStepLogSystem
帧同步日志系统，一个基于C#的高效帧同步日志系统，核心思路参考《腾讯游戏编程精粹》。 
测试用例放在了TestScript文件夹下。
## 使用方式(两种方式)
1. AutoInsert->自动插入日志代码。
该方法在原有的C#函数上插入日志语句，如果函数头里包含语句`FSPDebuger.IgnoreTrack()`，则不会有日志语句插入。
2. AutoInsert->IL注入日志代码  
这个方法利用IL注入(使用第三方cecil库)的方式，插入日志语句，不会污染C#代码。  
注入后的dll在`/Library/ScriptAssemblies/Test.dll`中，用ILSpy可以看到修改后代码。

## 日志的运行方式
1. PCMode  
单机模式下只会计算累加校验和
2. PVPMode  
PVP下会计算累加校验和，还有保留前50帧的压缩数据
3. CheckMode  
Check模式下计算累加校验和，还有一场战斗的所有日志