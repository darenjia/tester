from TSMasterAPI import *

# 初始化函数
initialize_lib_tsmaster(b'TSMasterTest')
# rpc句柄
rpchandle = size_t(0)
namelist = pchar()
# 获取正在运行的TSMaster进程名称 以;隔开 
get_active_application_list(namelist)
#传入的是c\c++ 类型字符串，因此需要解码一下
print(namelist.value.decode())
APPNameList = namelist.value.decode().split(';')

if len(APPNameList) !=0:
    # 创建本程序为client端，需要指定server的应用程序名
    # 因为是给到C/c++函数的参数，因此需要编码 
    ret = rpc_tsmaster_create_client(APPNameList[0].encode(), rpchandle); 
    # 激活客户端
    ret = rpc_tsmaster_activate_client(rpchandle,True); 
    # 设置系统变量
    # 参数2 填入你想设置的系统变量名称
    # 参数3 填入你想设置的系统变量的值
    ret = rpc_tsmaster_cmd_write_system_var(rpchandle,b'Var0',b'1000')
    
    # 注：信号值设置需要开启rbs 以及开启仿真
    # 开启仿真
    ret = rpc_tsmaster_cmd_start_simulation(rpchandle)
    
    # 读写can信号
    
    ret = rpc_tsmaster_cmd_set_can_signal(rpchandle,b'0/CAN_FD_Powertrain/Engine/Test_Message_CAN_FD/Test_Signal_Byte_00',100)
    # 最新版本TSMaster可以仅输入信号名 即可读写信号值 前提是开启rbs 以及 信号名没有重名
    ret = rpc_tsmaster_cmd_set_can_signal(rpchandle,b'Test_Signal_Byte_01_02',123)
    d = double(0)
    ret = rpc_tsmaster_cmd_get_can_signal(rpchandle,b'0/CAN_FD_Powertrain/Engine/Test_Message_CAN_FD/Test_Signal_Byte_00',d)
    print(d)
    ret = rpc_tsmaster_cmd_get_can_signal(rpchandle,b'Test_Signal_Byte_01_02',d)
    print(d)
    # 读写lin信号
    # 读写flexray信号


#释放资源
finalize_lib_tsmaster()
