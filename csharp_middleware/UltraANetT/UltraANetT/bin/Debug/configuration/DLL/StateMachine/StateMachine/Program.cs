using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using J1939_DUT_DataProcessW;
using CAN_DUT_DataProcessW;
using LIN_DUT_DataProcessW;


namespace StateMachine
{
    class Program
    {

        #region MDO4034C
        const String DllPath2 = "C:\\Users\\Administrator\\Desktop\\UltraANetT\\UltraANetT\\bin\\Debug\\configuration\\DLL\\MDO4034C\\Debug\\MDO4034C.dll";//DLL所在路径
        [DllImport(DllPath2, EntryPoint = "OSC_Set", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_Set(String IP);
        [DllImport(DllPath2, EntryPoint = "OSC_Initial", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_Initial();
        [DllImport(DllPath2, EntryPoint = "OSC_AutoSet", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_AutoSet(Int32 x);
        [DllImport(DllPath2, EntryPoint = "OSC_EnablePanel", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_EnablePanel(Int32 x);
        [DllImport(DllPath2, EntryPoint = "OSC_RunStop", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_RunStop(Int32 x);
        [DllImport(DllPath2, EntryPoint = "OSC_RunStopSequence", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_RunStopSequence(Int32 x);
        [DllImport(DllPath2, EntryPoint = "OSC_TriggerModel", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_TriggerModel(Int32 x);
        [DllImport(DllPath2, EntryPoint = "OSC_EdgeTrigger", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_EdgeTrigger(Int32 Channel, double Level, Int32 Mode, Int32 Coupl);
        [DllImport(DllPath2, EntryPoint = "OSC_PulseWidthTrigger", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_PulseWidthTrigger(Int32 Channel, double Level, Int32 Mode, double parm1, double parm2, Int32 polar);
        [DllImport(DllPath2, EntryPoint = "OSC_CANBitRateConfig", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_CANBitRateConfig(Int32 B, Int32 Baudrate);
        [DllImport(DllPath2, EntryPoint = "OSC_CANTriggerTypeFrame", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_CANTriggerTypeFrame(Int32 B, Int32 type, Int32 FrameType);
        [DllImport(DllPath2, EntryPoint = "OSC_CANTriggerIDDataFrame", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_CANTriggerIDDataFrame(Int32 B, Int32 Mode, UInt32 ID, Int32 DataSize, UInt64 DataValue);
        [DllImport(DllPath2, EntryPoint = "OSC_CANTriggerDataFrame", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_CANTriggerDataFrame(Int32 B, Int32 DataSize, UInt64 DataValue);
        [DllImport(DllPath2, EntryPoint = "OSC_CANTriggerID", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_CANTriggerID(Int32 B, Int32 Mode, UInt32 ID);
        [DllImport(DllPath2, EntryPoint = "OSC_CANConfig", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_CANConfig(Int32 B, Int32 Source, Int32 BitRate, Int32 SampPoint, Int32 DisplayFormat, Int32 DisplayType, Int32 Position, double TrigLevel, Int32 signaltype);
        [DllImport(DllPath2, EntryPoint = "OSC_BusDisplay", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_BusDisplay(Int32 B, Int32 Display);
        [DllImport(DllPath2, EntryPoint = "OSC_Single", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_Single(Int32 Mode);
        [DllImport(DllPath2, EntryPoint = "OSC_Cursor", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_Cursor(Int32 Channel, Int32 Mode, double Xa, double Xb, double Ya, double Yb);
        [DllImport(DllPath2, EntryPoint = "OSC_CursorLink", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_CursorLink(Int32 Select);
        [DllImport(DllPath2, EntryPoint = "OSC_MeasurClear", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_MeasurClear();
        [DllImport(DllPath2, EntryPoint = "OSC_MeasurAdd", CallingConvention = CallingConvention.Cdecl)]
        extern static double OSC_MeasurAdd(Int32 Channel, Int32 Select);
        [DllImport(DllPath2, EntryPoint = "OSC_MeasurRise", CallingConvention = CallingConvention.Cdecl)]
        extern static double OSC_MeasurRise(Int32 Channel);
        [DllImport(DllPath2, EntryPoint = "OSC_MeasurFall", CallingConvention = CallingConvention.Cdecl)]
        extern static double OSC_MeasurFall(Int32 Channel);
        [DllImport(DllPath2, EntryPoint = "OSC_TimeBase", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_TimeBase(double Timebase, Int32 Position, Int32 RecordLength);
        [DllImport(DllPath2, EntryPoint = "OSC_AcquireWave", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_AcquireWave(Int32 Channel, String Path);
        [DllImport(DllPath2, EntryPoint = "OSC_SaveImage", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_SaveImage(String Path, Int32 Mode);
        [DllImport(DllPath2, EntryPoint = "OSC_MathOn", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_MathOn(Int32 channel1, Int32 channel2, Int32 math);
        [DllImport(DllPath2, EntryPoint = "OSC_MathOff", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_MathOff();
        [DllImport(DllPath2, EntryPoint = "OSC_ChannelDisplay", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_ChannelDisplay(Int32 Channel, Int32 Display);
        [DllImport(DllPath2, EntryPoint = "OSC_LINTrigID", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_LINTrigID(Int32 B, UInt32 ID);
        [DllImport(DllPath2, EntryPoint = "OSC_LINTrigDataFrame", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_LINTrigDataFrame(Int32 B, Int32 DataSize, UInt64 DataValue);
        [DllImport(DllPath2, EntryPoint = "OSC_LINTrigIDDataFrame", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_LINTrigIDDataFrame(Int32 B, UInt32 ID, Int32 DataSize, UInt64 DataValue);
        [DllImport(DllPath2, EntryPoint = "OSC_LINTrigError", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_LINTrigError(Int32 B, Int32 ErrorMode);
        [DllImport(DllPath2, EntryPoint = "OSC_LINConfig", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_LINConfig(Int32 B, Int32 Channel, Int32 BitRate, double Level);
        [DllImport(DllPath2, EntryPoint = "OSC_LINBitRateConfig", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_LINBitRateConfig(Int32 B, Int32 Baudrate);
        [DllImport(DllPath2, EntryPoint = "OSC_TriggerLevel", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_TriggerLevel(Int32 source, double Level);
        [DllImport(DllPath2, EntryPoint = "OSC_AcquireConfig", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_AcquireConfig(Int32 RecordLength);
        [DllImport(DllPath2, EntryPoint = "OSC_MeasurVoltage", CallingConvention = CallingConvention.Cdecl)]
        extern static double OSC_MeasurVoltage(Int32 Channel, double Xa, double Xb);
        [DllImport(DllPath2, EntryPoint = "OSC_LINTrigMode", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_LINTrigMode(Int32 B, Int32 Mode);
        [DllImport(DllPath2, EntryPoint = "OSC_ChannelSet", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_ChannelSet(Int32 Channel, double Range, double Offset);
        [DllImport(DllPath2, EntryPoint = "OSC_ZoomCfg", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_ZoomCfg(double Timebase);
        [DllImport(DllPath2, EntryPoint = "OSC_ZoomClose", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_ZoomClose();
        [DllImport(DllPath2, EntryPoint = "OSC_ZoomPosition", CallingConvention = CallingConvention.Cdecl)]
        extern static UInt32 OSC_ZoomPosition(double Position);
        #endregion

        //全局变量定义部分
        #region COM接口变量
        static CANoe.Application mCANoeApp;
        static CANoe.EnvironmentVariable EnVariable_CAPL_TIME;
        static CANoe.EnvironmentVariable EnVariable_CAPL_String;
        static CANoe.EnvironmentVariable EnVariable_StateMachine_String;
        static CANoe.EnvironmentVariable EnVariable_StateMachine_TIME;
        #endregion
        #region 自定义变量
        static double CAPL_TIME = 0;//用于记录上一次CAPL传递的时间参数
        static String CAPL_String = "";//用于记录CAPL本次传递的字符串
        static String CAPL_FunctionName = "";//记录CAPL本次运行的函数名称
        static String StateMachine_String = "";//记录StateMachine本次运行传递的字符串
        static DateTime StateMachine_TIME;//用于获取当前系统时间
        static int Pos1 = 0;
        static int Pos2 = 0;
        static int Pos3 = 0;
        static int Pos4 = 0;
        static int Pos5 = 0;
        static int Pos6 = 0;
        static int Pos7 = 0;
        static int Pos8 = 0;
        static int Pos9 = 0;
        static int Pos10 = 0;
        static int Pos11 = 0; 
        static int Pos12 = 0;
        #endregion

        static void Main(string[] args)
        {

            while (true)
            {
                mCANoeApp = new CANoe.Application();
                EnVariable_CAPL_TIME = mCANoeApp.Environment.GetVariable("CAPL_TIME");
                EnVariable_CAPL_String = mCANoeApp.Environment.GetVariable("CAPL_String");//CAPL_String环境变量格式定义：函数名称@变量1@变量2...@
                EnVariable_StateMachine_String = mCANoeApp.Environment.GetVariable("StateMachine_String");//StateMachine_String环境变量格式定义：函数返回值@变量1@变量2...@
                EnVariable_StateMachine_TIME = mCANoeApp.Environment.GetVariable("StateMachine_TIME");
                //Console.WriteLine("输入 CAPL_String:");
                //CAPL_String=Console.ReadLine();
                if (EnVariable_CAPL_TIME.Value != 0 || EnVariable_CAPL_String.Value.ToString() != "")
                {
                    if (EnVariable_CAPL_TIME.Value != CAPL_TIME)
                    {
                        CAPL_String = EnVariable_CAPL_String.Value.ToString();
                        Pos1 = CAPL_String.IndexOf("@", 0);
                        if (Pos1 > 0)
                        {
                            CAPL_FunctionName = CAPL_String.Substring(0, Pos1);
                            //CAN_DUT_DataProcessW状态机主体
                            #region ANetT_Database_CAN_File
                            if (CAPL_FunctionName == "ANetT_Database_CAN_File")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Console.WriteLine("DBC路径 Path:{0}",Path);
                                    Int32 Re = A_CD_DP.ANetT_Database_CAN_File(Path);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Database_CAN_TXID
                            if (CAPL_FunctionName == "ANetT_Database_CAN_TXID")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    String DUTName = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    UInt32[] ID = new UInt32[500];
                                    Int32 Re = A_CD_DP.ANetT_Database_CAN_TXID(DUTName, ID);
                                    String Temp = Re.ToString() + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        Temp = Temp + ID[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";
                                    EnVariable_StateMachine_String.Value = Temp;
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Database_CAN_RXID
                            if (CAPL_FunctionName == "ANetT_Database_CAN_RXID")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    String DUTName = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    UInt32[] ID = new UInt32[500];
                                    Int32 Re = A_CD_DP.ANetT_Database_CAN_RXID(DUTName, ID);
                                    String Temp = Re.ToString() + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        Temp = Temp + ID[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";
                                    EnVariable_StateMachine_String.Value = Temp;
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Database_CAN_IDCycletimeDLC
                            if (CAPL_FunctionName == "ANetT_Database_CAN_IDCycletimeDLC")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    String IDTemp = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    UInt32[] ID = new UInt32[500];
                                    Pos3 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (IDTemp.IndexOf("#", Pos3 + 1) <= 0) break;
                                        ID[i] = Convert.ToUInt32(IDTemp.Substring(Pos3 + 1, IDTemp.IndexOf("#", Pos3 + 1) - Pos3 - 1), 16);
                                        Pos3 = IDTemp.IndexOf("#", Pos3 + 1);
                                    }
                                    Int32[] DLC = new Int32[500];
                                    Int32[] Cycletime = new Int32[500];
                                    Int32 Re = A_CD_DP.ANetT_Database_CAN_IDCycletimeDLC(ID, DLC, Cycletime);
                                    String Temp = Re.ToString() + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        Temp = Temp + DLC[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        Temp = Temp + Cycletime[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";
                                    EnVariable_StateMachine_String.Value = Temp;
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Database_CAN_IDDLC
                            if (CAPL_FunctionName == "ANetT_Database_CAN_IDDLC")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    String IDTemp = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    UInt32[] ID = new UInt32[500];
                                    Pos3 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (IDTemp.IndexOf("#", Pos3 + 1) <= 0) break;
                                        ID[i] = Convert.ToUInt32(IDTemp.Substring(Pos3 + 1, IDTemp.IndexOf("#", Pos3 + 1) - Pos3 - 1), 16);
                                        Pos3 = IDTemp.IndexOf("#", Pos3 + 1);
                                    }
                                    Int32[] DLC = new Int32[500];
                                    Int32 Re = A_CD_DP.ANetT_Database_CAN_IDDLC(ID, DLC);
                                    String Temp = Re.ToString() + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        Temp = Temp + DLC[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";
                                    EnVariable_StateMachine_String.Value = Temp;
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Database_CAN_IDCycletime
                            if (CAPL_FunctionName == "ANetT_Database_CAN_IDCycletime")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    String IDTemp = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    UInt32[] ID = new UInt32[500];
                                    Pos3 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (IDTemp.IndexOf("#", Pos3 + 1) <= 0) break;
                                        ID[i] = Convert.ToUInt32(IDTemp.Substring(Pos3 + 1, IDTemp.IndexOf("#", Pos3 + 1) - Pos3 - 1), 16);
                                        Pos3 = IDTemp.IndexOf("#", Pos3 + 1);
                                    }
                                    Int32[] Cycletime = new Int32[500];
                                    Int32 Re = A_CD_DP.ANetT_Database_CAN_IDCycletime(ID, Cycletime);
                                    String Temp = Re.ToString() + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        Temp = Temp + Cycletime[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";
                                    EnVariable_StateMachine_String.Value = Temp;
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Database_CAN_MsgAll
                            if (CAPL_FunctionName == "ANetT_Database_CAN_MsgAll")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    String DUTName = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    UInt32[] ID = new UInt32[500];
                                    Int32[] signal_N = new Int32[500];
                                    Int32[,] startbit = new Int32[500, 100];
                                    Int32[,] length = new Int32[500, 100];
                                    Int32[,] method = new Int32[500, 100];
                                    Int32 Re = 0;
                                    int row1 = startbit.GetUpperBound(0) + 1;
                                    int col1 = startbit.GetUpperBound(1) + 1;
                                    int row2 = length.GetUpperBound(0) + 1;
                                    int col2 = length.GetUpperBound(1) + 1;
                                    int row3 = method.GetUpperBound(0) + 1;
                                    int col3 = method.GetUpperBound(1) + 1;
                                    unsafe
                                    {
                                        fixed (Int32* fp1 = startbit)
                                        {
                                            fixed (Int32* fp2 = length)
                                            {
                                                fixed (Int32* fp3 = method)
                                                {
                                                    Int32*[] fstartbit = new Int32*[500];
                                                    Int32*[] flength = new Int32*[500];
                                                    Int32*[] fmethod = new Int32*[500];
                                                    for (int i = 0; i < row1; i++)
                                                    {
                                                        fstartbit[i] = fp1 + i * col1;
                                                    }
                                                    for (int i = 0; i < row2; i++)
                                                    {
                                                        flength[i] = fp2 + i * col2;
                                                    }
                                                    for (int i = 0; i < row3; i++)
                                                    {
                                                        fmethod[i] = fp3 + i * col3;
                                                    }
                                                    Re = A_CD_DP.ANetT_Database_CAN_MsgAll(DUTName, ID, signal_N, fstartbit, flength, fmethod);
                                                }
                                            }
                                        }
                                    }
                                    //String Temp = Re.ToString() + "@" + "ID如下" + "\r\n";
                                    String Temp = Re.ToString() + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        Temp = Temp + ID[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";// +"ID信号个数如下" + "\r\n";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        Temp = Temp + signal_N[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";// +"ID下信号的起始位如下" + "\r\n";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        for (int j = 0; j < signal_N[i]; j++)
                                        {
                                            Temp = Temp + startbit[i, j].ToString() + "$";
                                        }
                                        Temp = Temp + "#";
                                    }
                                    Temp = Temp + "@";// +"ID信号长度如下" + "\r\n";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        for (int j = 0; j < signal_N[i]; j++)
                                        {
                                            Temp = Temp + length[i, j].ToString() + "$";
                                        }
                                        Temp = Temp + "#";
                                    }
                                    Temp = Temp + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        for (int j = 0; j < signal_N[i]; j++)
                                        {
                                            Temp = Temp + method[i, j].ToString() + "$";
                                        }
                                        Temp = Temp + "#";
                                    }
                                    Temp = Temp + "@";
                                    EnVariable_StateMachine_String.Value = Temp;
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Bus_CAN_AcquireID
                            if (CAPL_FunctionName == "ANetT_Bus_CAN_AcquireID")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    String Path = CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1);
                                    UInt32[] ID = new UInt32[500];
                                    Int32[] DLC = new Int32[500];
                                    Int32 Re = A_CD_DP.ANetT_Bus_CAN_AcquireID(Channel, Path, ID);
                                    String Temp = Re.ToString() + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        Temp = Temp + ID[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";
                                    //for (int i = 0; i < Re; i++)
                                    //{
                                    //    Temp = Temp + DLC[i].ToString() + "#";
                                    //}
                                    //Temp = Temp + "@";
                                    EnVariable_StateMachine_String.Value = Temp;
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion


                            #region ANetT_Test_CAN_InitalSequenceTest
                            if (CAPL_FunctionName == "ANetT_Test_CAN_InitalSequenceTest")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    string Describe = "";
                                    double Re = A_CD_DP.ANetT_Test_CAN_InitalSequenceTest(Path, Channel, ref Describe);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describe + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_CAN_StandardFrameType
                            if (CAPL_FunctionName == "ANetT_Test_CAN_StandardFrameType")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    string Describ = "";
                                    Int32 Re = A_CD_DP.ANetT_Test_CAN_StandardFrameType(Path, Channel, ref Describ);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describ + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_CAN_IDCheck
                            if (CAPL_FunctionName == "ANetT_Test_CAN_IDCheck")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    String StandardIDTemp = CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1);
                                    UInt32[] StandardID = new UInt32[500];
                                    Pos5 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (StandardIDTemp.IndexOf("#", Pos5 + 1) <= 0) break;
                                        StandardID[i] = Convert.ToUInt32(StandardIDTemp.Substring(Pos5 + 1, StandardIDTemp.IndexOf("#", Pos5 + 1) - Pos5 - 1), 16);
                                        Pos5 = StandardIDTemp.IndexOf("#", Pos5 + 1);
                                    }
                                    string Describ = "";
                                    Int32 Re = A_CD_DP.ANetT_Test_CAN_IDCheck(Path, Channel, StandardID, ref Describ);

                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describ + "@";

                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_CAN_DLCCheck
                            if (CAPL_FunctionName == "ANetT_Test_CAN_DLCCheck")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0 && Pos5 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    String StandardIDTemp = CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1);
                                    String StandardDLCTemp = CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1);
                                    UInt32[] StandardID = new UInt32[500];
                                    Pos6 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (StandardIDTemp.IndexOf("#", Pos6 + 1) <= 0) break;
                                        StandardID[i] = Convert.ToUInt32(StandardIDTemp.Substring(Pos6 + 1, StandardIDTemp.IndexOf("#", Pos6 + 1) - Pos6 - 1), 16);
                                        Pos6 = StandardIDTemp.IndexOf("#", Pos6 + 1);
                                    }
                                    Int32[] StandardDLC = new Int32[500];
                                    Pos7 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (StandardDLCTemp.IndexOf("#", Pos7 + 1) <= 0) break;
                                        StandardDLC[i] = Convert.ToInt32(StandardDLCTemp.Substring(Pos7 + 1, StandardDLCTemp.IndexOf("#", Pos7 + 1) - Pos7 - 1), 16);
                                        Pos7 = StandardDLCTemp.IndexOf("#", Pos7 + 1);
                                    }
                                    string Describ = "";
                                    Int32 Re = A_CD_DP.ANetT_Test_CAN_DLCCheck(Path, Channel, StandardID, StandardDLC, ref Describ);

                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describ + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_CAN_CycleCheck
                            if (CAPL_FunctionName == "ANetT_Test_CAN_CycleCheck")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0 && Pos5 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    String StandardIDTemp = CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1);
                                    String StandardCycleTimeTemp = CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1);
                                    UInt32[] StandardID = new UInt32[500];
                                    Pos6 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (StandardIDTemp.IndexOf("#", Pos6 + 1) <= 0) break;
                                        StandardID[i] = Convert.ToUInt32(StandardIDTemp.Substring(Pos6 + 1, StandardIDTemp.IndexOf("#", Pos6 + 1) - Pos6 - 1), 16);
                                        Pos6 = StandardIDTemp.IndexOf("#", Pos6 + 1);
                                    }
                                    Int32[] StandardCycleTime = new Int32[500];
                                    Pos7 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (StandardCycleTimeTemp.IndexOf("#", Pos7 + 1) <= 0) break;
                                        StandardCycleTime[i] = Convert.ToInt32(StandardCycleTimeTemp.Substring(Pos7 + 1, StandardCycleTimeTemp.IndexOf("#", Pos7 + 1) - Pos7 - 1), 10);
                                        Pos7 = StandardCycleTimeTemp.IndexOf("#", Pos7 + 1);
                                    }
                                    string Describ = "";
                                    Int32 Re = A_CD_DP.ANetT_Test_CAN_CycleCheck(Path, Channel, StandardID, StandardCycleTime, ref Describ);

                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describ + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_CAN_ErrorFrameCheck
                            if (CAPL_FunctionName == "ANetT_Test_CAN_ErrorFrameCheck")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    Int32 MsgFlag = 0;
                                    Int32 ErrFlag = 0;
                                    double Re = A_CD_DP.ANetT_Test_CAN_ErrorFrameCheck(Path, Channel, ref MsgFlag, ref ErrFlag);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + MsgFlag.ToString() + "@" + ErrFlag.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_CAN_Tbit
                            if (CAPL_FunctionName == "ANetT_Test_CAN_Tbit")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);
                                Pos6 = CAPL_String.IndexOf("@", Pos5 + 1);
                                Pos7 = CAPL_String.IndexOf("@", Pos6 + 1);
                                Pos8 = CAPL_String.IndexOf("@", Pos7 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0 && Pos5 > 0 && Pos6 > 0 && Pos7 > 0 && Pos8 > 0)
                                {
                                    String Path1 = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    String Path2 = CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1);
                                    double Timebase = Convert.ToDouble(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    Int32 BaudRate = Convert.ToInt32(CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1));
                                    Int32 SampPoint = Convert.ToInt32(CAPL_String.Substring(Pos5 + 1, Pos6 - Pos5 - 1));
                                    Int32 DLC = Convert.ToInt32(CAPL_String.Substring(Pos6 + 1, Pos7 - Pos6 - 1));
                                    UInt32 ID = Convert.ToUInt32(CAPL_String.Substring(Pos7 + 1, Pos8 - Pos7 - 1), 16);
                                    double CurSorA = 0;
                                    double CurSorB = 0;
                                    double Re = A_CD_DP.ANetT_Test_CAN_Tbit(Path1, Path2, Timebase, BaudRate, SampPoint, DLC, ID, ref CurSorA, ref CurSorB);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + CurSorA.ToString() + "@" + CurSorB.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_CAN_Symmetry
                            if (CAPL_FunctionName == "ANetT_Test_CAN_Symmetry")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    double Timebase = Convert.ToDouble(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    Int32 SampPoint = Convert.ToInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    double CurSorA = 0;
                                    double CurSorB = 0;
                                    double Re = A_CD_DP.ANetT_Test_CAN_Symmetry(Path, Timebase, SampPoint, ref CurSorA, ref CurSorB);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + CurSorA.ToString() + "@" + CurSorB.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_CAN_SamplingPoint
                            if (CAPL_FunctionName == "ANetT_Test_CAN_SamplingPoint")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    String Path = CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1);
                                    Int32 Re = A_CD_DP.ANetT_Test_CAN_SamplingPoint(Channel, Path);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_CAN_InitialorFaultRecoverTime_OSC
                            if (CAPL_FunctionName == "ANetT_Test_CAN_InitialorFaultRecoverTime_OSC")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0 && Pos5 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    double Timebase = Convert.ToDouble(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    Int32 SampPoint = Convert.ToInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    double TrrggerV = Convert.ToDouble(CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1));
                                    double Re = A_CD_DP.ANetT_Test_CAN_InitialorFaultRecoverTime_OSC(Path, Timebase, SampPoint, TrrggerV);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_CAN_InitialorFaultRecoverTime_file
                            if (CAPL_FunctionName == "ANetT_Test_CAN_InitialorFaultRecoverTime_file")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    double Re = A_CD_DP.ANetT_Test_CAN_InitialorFaultRecoverTime_file(Path, Channel);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_CAN_StuffBit
                            if (CAPL_FunctionName == "ANetT_Test_CAN_StuffBit")//路径@信号编码方式@通道@ID@信号个数@信号起始位@信号长度@@@
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);//路径
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);//信号编码方式
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);//通道
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);//ID
                                Pos6 = CAPL_String.IndexOf("@", Pos5 + 1);//信号个数
                                Pos7 = CAPL_String.IndexOf("@", Pos6 + 1);//信号起始位
                                Pos8 = CAPL_String.IndexOf("@", Pos7 + 1);//信号起长度

                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0 && Pos5 > 0 && Pos6 > 0 && Pos7 > 0 && Pos8 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 ByteOrder = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    String StandardIDTemp = CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1);
                                    String StandardDSignalnumTemp = CAPL_String.Substring(Pos5 + 1, Pos6 - Pos5 - 1);
                                    String StandardStartBitTemp = CAPL_String.Substring(Pos6 + 1, Pos7 - Pos6 - 1);
                                    String StandardLengthTemp = CAPL_String.Substring(Pos7 + 1, Pos8 - Pos7 - 1);
                                    UInt32[] StandardID = new UInt32[500];
                                    UInt32[] StandardSignalnum = new UInt32[500];
                                    UInt32[,] StandardStartBit = new UInt32[200, 500];
                                    UInt32[,] StandardLength = new UInt32[200, 500];

                                    Pos9 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (StandardIDTemp.IndexOf("#", Pos9 + 1) <= 0) break;
                                        StandardID[i] = UInt32.Parse((StandardIDTemp.Substring(Pos9 + 1, StandardIDTemp.IndexOf("#", Pos9 + 1) - Pos9 - 1)), System.Globalization.NumberStyles.HexNumber);
                                        Pos9 = StandardIDTemp.IndexOf("#", Pos9 + 1);
                                    }
                                    Pos10 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (StandardDSignalnumTemp.IndexOf("#", Pos10 + 1) <= 0) break;
                                        StandardSignalnum[i] = Convert.ToUInt32(StandardDSignalnumTemp.Substring(Pos10 + 1, StandardDSignalnumTemp.IndexOf("#", Pos10 + 1) - Pos10 - 1));
                                        //  StandardSignalnum[i] = UInt32.Parse((StandardDSignalnumTemp.Substring(Pos10 + 1, StandardDSignalnumTemp.IndexOf("#", Pos10 + 1) - Pos10 - 1)), System.Globalization.NumberStyles.HexNumber);
                                        Pos10 = StandardDSignalnumTemp.IndexOf("#", Pos10 + 1);
                                    }
                                    Pos11 = -1;
                                    for (int i = 0; StandardID[i] != 0; i++)
                                    {
                                        for (int j = 0; j < StandardSignalnum[i]; j++)
                                        {
                                            if (StandardStartBitTemp.IndexOf("#", Pos11 + 1) <= 0) break;
                                            StandardStartBit[i, j] = Convert.ToUInt32(StandardStartBitTemp.Substring(Pos11 + 1, StandardStartBitTemp.IndexOf("#", Pos11 + 1) - Pos11 - 1));
                                            //  StandardStartBit[i, j] = UInt32.Parse((StandardStartBitTemp.Substring(Pos11 + 1, StandardStartBitTemp.IndexOf("#", Pos11 + 1) - Pos11 - 1)), System.Globalization.NumberStyles.HexNumber);
                                            Pos11 = StandardStartBitTemp.IndexOf("#", Pos11 + 1);
                                        }
                                    }
                                    Pos12 = -1;
                                    for (int i = 0; StandardID[i] != 0; i++)
                                    {
                                        for (int j = 0; j < StandardSignalnum[i]; j++)
                                        {
                                            if (StandardLengthTemp.IndexOf("#", Pos12 + 1) <= 0) break;
                                            StandardLength[i, j] = Convert.ToUInt32(StandardLengthTemp.Substring(Pos12 + 1, StandardLengthTemp.IndexOf("#", Pos12 + 1) - Pos12 - 1));
                                            // StandardLength[i, j] = UInt32.Parse((StandardLengthTemp.Substring(Pos12 + 1, StandardLengthTemp.IndexOf("#", Pos12 + 1) - Pos12 - 1)), System.Globalization.NumberStyles.HexNumber);
                                            Pos12 = StandardLengthTemp.IndexOf("#", Pos12 + 1);
                                        }
                                    }

                                    string Describ = "";
                                    Int32 Re = A_CD_DP.ANetT_Test_CAN_StuffBit(Path, ByteOrder, Channel, StandardID, StandardSignalnum, StandardStartBit, StandardLength, ref Describ);

                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describ + "@";

                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion


                            #region ANetT_Test_CAN_BusOffCheck
                            if (CAPL_FunctionName == "ANetT_Test_CAN_BusOffCheck")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    Int32 number = Convert.ToInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    string Describ = "";
                                    Int32 Re = A_CD_DP.ANetT_Test_CAN_BusOffCheck(Path, Channel, number, ref Describ);

                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describ + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_CAN_Holdtime
                            if (CAPL_FunctionName == "ANetT_Test_CAN_Holdtime")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    double Timebase = Convert.ToDouble(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    Int32 SampPoint = Convert.ToInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    double Re = A_CD_DP.ANetT_Test_CAN_Holdtime(Path, Timebase, SampPoint);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_CAN_Starttime
                            if (CAPL_FunctionName == "ANetT_Test_CAN_Starttime")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    double Timebase = Convert.ToDouble(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    Int32 SampPoint = Convert.ToInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    double Re = A_CD_DP.ANetT_Test_CAN_Starttime(Path, Timebase, SampPoint);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion

                            //暂时不用
                            #region ANetT_Test_CAN_ExtendFrameType
                            //if (CAPL_FunctionName == "ANetT_Test_CAN_ExtendFrameType")
                            //{
                            //    Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                            //    Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                            //    if (Pos2 > 0 && Pos3 > 0)
                            //    {
                            //        String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                            //        Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                            //        string Describ = "";
                            //        Int32 Re = A_CD_DP.ANetT_Test_CAN_ExtendFrameType(Path, Channel, ref Describ);
                            //        EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describ + "@";
                            //        StateMachine_TIME = DateTime.Now;
                            //        EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                            //    }
                            //    else
                            //    {
                            //        EnVariable_StateMachine_String.Value = "InputError";
                            //        StateMachine_TIME = DateTime.Now;
                            //        EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                            //    }
                            //}
                            #endregion
                            #region ANetT_Test_CAN_InitialTime
                            //if (CAPL_FunctionName == "ANetT_Test_CAN_InitialTime")
                            //{
                            //    Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                            //    Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                            //    if (Pos2 > 0 && Pos3 > 0)
                            //    {
                            //        String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                            //        Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                            //        double Re = A_CD_DP.ANetT_Test_CAN_InitialTime(Path, Channel);
                            //        EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                            //        StateMachine_TIME = DateTime.Now;
                            //        EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                            //    }
                            //    else
                            //    {
                            //        EnVariable_StateMachine_String.Value = "InputError";
                            //        StateMachine_TIME = DateTime.Now;
                            //        EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                            //    }
                            //}
                            #endregion
                            #region ANetT_Test_CAN_ErrorFrameCheck
                            if (CAPL_FunctionName == "ANetT_Test_CAN_ErrorFrameCheck")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    Int32 MsgFlag = 0;
                                    Int32 ErrFlag = 0;
                                    double Re = A_CD_DP.ANetT_Test_CAN_ErrorFrameCheck(Path, Channel, ref MsgFlag, ref ErrFlag);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + MsgFlag.ToString() + "@" + ErrFlag.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_CAN_Starttime
                            if (CAPL_FunctionName == "ANetT_Test_CAN_Starttime")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    double Timebase = Convert.ToDouble(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    Int32 SampPoint = Convert.ToInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    double Re = A_CD_DP.ANetT_Test_CAN_Starttime(Path, Timebase, SampPoint);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion

                            //J1939_DUT_DataProcessW状态机主体
                            #region ANetT_Database_J1939_File
                            if (CAPL_FunctionName == "ANetT_Database_J1939_File")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 Re = A_J1939_DP.ANetT_Database_J1939_File(Path);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Database_J1939_TXID
                            if (CAPL_FunctionName == "ANetT_Database_J1939_TXID")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    String DUTName = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    UInt32[] ID = new UInt32[500];
                                    Int32 Re = A_J1939_DP.ANetT_Database_J1939_TXID(DUTName, ID);
                                    String Temp = Re.ToString() + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                         Temp = Temp + ID[i].ToString() + "#";
                                        //Temp = Temp + "0x" + ID[i].ToString("x8").ToUpper() + "x" + "#";
                                    }
                                    Temp = Temp + "@";
                                    EnVariable_StateMachine_String.Value = Temp;
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Database_J1939_RXID
                            if (CAPL_FunctionName == "ANetT_Database_J1939_RXID")
                            {

                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    String DUTName = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    UInt32[] ID = new UInt32[500];
                                    Int32 Re = A_J1939_DP.ANetT_Database_J1939_RXID(DUTName, ID);
                                    String Temp = Re.ToString() + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                         Temp = Temp + ID[i].ToString() + "#";
                                        //Temp = Temp + "0x" + ID[i].ToString("x8").ToUpper() + "x" + "#";
                                    }
                                    Temp = Temp + "@";
                                    EnVariable_StateMachine_String.Value = Temp;
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Database_J1939_IDCycletimeDLC
                            if (CAPL_FunctionName == "ANetT_Database_J1939_IDCycletimeDLC")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    String IDTemp = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    UInt32[] ID = new UInt32[500];
                                    Pos3 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (IDTemp.IndexOf("#", Pos3 + 1) <= 0) break;
                                        // ID[i] = Convert.ToUInt32(IDTemp.Substring(Pos3 + 1, IDTemp.IndexOf("#", Pos3 + 1) - Pos3 - 1));
                                        ID[i] = UInt32.Parse((IDTemp.Substring(Pos3 + 1, IDTemp.IndexOf("#", Pos3 + 1) - Pos3 - 1)), System.Globalization.NumberStyles.HexNumber);

                                        Pos3 = IDTemp.IndexOf("#", Pos3 + 1);
                                    }
                                    Int32[] DLC = new Int32[500];
                                    Int32[] Cycletime = new Int32[500];
                                    Int32 Re = A_J1939_DP.ANetT_Database_J1939_IDCycletimeDLC(ID, DLC, Cycletime);
                                    String Temp = Re.ToString() + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        Temp = Temp + DLC[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        Temp = Temp + Cycletime[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";
                                    EnVariable_StateMachine_String.Value = Temp;
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Database_J1939_IDDLC
                            if (CAPL_FunctionName == "ANetT_Database_J1939_IDDLC")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    String IDTemp = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    UInt32[] ID = new UInt32[500];
                                    Pos3 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (IDTemp.IndexOf("#", Pos3 + 1) <= 0) break;
                                        ID[i] = Convert.ToUInt32(IDTemp.Substring(Pos3 + 1, IDTemp.IndexOf("#", Pos3 + 1) - Pos3 - 1), 16);
                                        Pos3 = IDTemp.IndexOf("#", Pos3 + 1);
                                    }
                                    Int32[] DLC = new Int32[500];
                                    Int32 Re = A_J1939_DP.ANetT_Database_J1939_IDDLC(ID, DLC);
                                    String Temp = Re.ToString() + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        Temp = Temp + DLC[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";
                                    EnVariable_StateMachine_String.Value = Temp;
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Database_J1939_IDCycletime
                            if (CAPL_FunctionName == "ANetT_Database_J1939_IDCycletime")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    String IDTemp = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    UInt32[] ID = new UInt32[500];
                                    Pos3 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (IDTemp.IndexOf("#", Pos3 + 1) <= 0) break;
                                        ID[i] = Convert.ToUInt32(IDTemp.Substring(Pos3 + 1, IDTemp.IndexOf("#", Pos3 + 1) - Pos3 - 1), 16);
                                        Pos3 = IDTemp.IndexOf("#", Pos3 + 1);
                                    }
                                    Int32[] Cycletime = new Int32[500];
                                    Int32 Re = A_J1939_DP.ANetT_Database_J1939_IDCycletime(ID, Cycletime);
                                    String Temp = Re.ToString() + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        Temp = Temp + Cycletime[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";
                                    EnVariable_StateMachine_String.Value = Temp;
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Bus_J1939_AcquireID
                            if (CAPL_FunctionName == "ANetT_Bus_J1939_AcquireID")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    String Path = CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1);
                                    UInt32[] ID = new UInt32[500];
                                    Int32[] DLC = new Int32[500];
                                    Int32 Re = A_J1939_DP.ANetT_Bus_J1939_AcquireID(Channel, Path, ID);
                                    String Temp = Re.ToString() + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        Temp = Temp + ID[i].ToString() + "#";
                                        //Temp = Temp + "0x" + ID[i].ToString("x8").ToUpper() + "x" + "#";
                                    }
                                    Temp = Temp + "@";
                                    //for (int i = 0; i < Re; i++)
                                    //{
                                    //    Temp = Temp + DLC[i].ToString() + "#";
                                    //}
                                    //Temp = Temp + "@";
                                    EnVariable_StateMachine_String.Value = Temp;
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_J1939_InitalSequenceTest
                            if (CAPL_FunctionName == "ANetT_Test_J1939_InitalSequenceTest")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    string Describ = "";
                                    double Re = A_J1939_DP.ANetT_Test_J1939_InitalSequenceTest(Path, Channel, ref Describ);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describ + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_J1939_StandardFrameType
                            //if (CAPL_FunctionName == "ANetT_Test_J1939_StandardFrameType")
                            //{
                            //    Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                            //    Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                            //    if (Pos2 > 0 && Pos3 > 0)
                            //    {
                            //        String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                            //        Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                            //        string Describ = "";
                            //        Int32 Re = A_J1939_DP.ANetT_Test_J1939_StandardFrameType(Path, Channel, ref Describ);
                            //        EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describ + "@";
                            //        StateMachine_TIME = DateTime.Now;
                            //        EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                            //    }
                            //    else
                            //    {
                            //        EnVariable_StateMachine_String.Value = "InputError";
                            //        StateMachine_TIME = DateTime.Now;
                            //        EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                            //    }
                            //}
                            #endregion
                            #region ANetT_Test_J1939_ExtendFrameType
                            if (CAPL_FunctionName == "ANetT_Test_J1939_ExtendFrameType")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    string Describ = "";
                                    Int32 Re = A_J1939_DP.ANetT_Test_J1939_ExtendFrameType(Path, Channel, ref Describ);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describ + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_J1939_InitialorFaultRecoverTime_file
                            if (CAPL_FunctionName == "ANetT_Test_J1939_InitialorFaultRecoverTime_file")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    double Re = A_J1939_DP.ANetT_Test_J1939_InitialorFaultRecoverTime_file(Path, Channel);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_J1939_ErrorFrameCheck
                            //if (CAPL_FunctionName == "ANetT_Test_J1939_ErrorFrameCheck")
                            //{
                            //    Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                            //    Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                            //    if (Pos2 > 0 && Pos3 > 0)
                            //    {
                            //        String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                            //        Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                            //        Int32 MsgFlag = 0;
                            //        Int32 ErrFlag = 0;
                            //        double Re = A_J1939_DP.ANetT_Test_J1939_ErrorFrameCheck(Path, Channel, ref MsgFlag, ref ErrFlag);
                            //        EnVariable_StateMachine_String.Value = Re.ToString() + "@" + MsgFlag.ToString() + "@" + ErrFlag.ToString() + "@";
                            //        StateMachine_TIME = DateTime.Now;
                            //        EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                            //    }
                            //    else
                            //    {
                            //        EnVariable_StateMachine_String.Value = "InputError";
                            //        StateMachine_TIME = DateTime.Now;
                            //        EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                            //    }
                            //}
                            #endregion
                            #region ANetT_Database_J1939_MsgAll
                            if (CAPL_FunctionName == "ANetT_Database_J1939_MsgAll")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    String DUTName = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    UInt32[] ID = new UInt32[500];
                                    Int32[] signal_N = new Int32[500];
                                    Int32[,] startbit = new Int32[500, 100];
                                    Int32[,] length = new Int32[500, 100];
                                    Int32[,] method = new Int32[500, 100];
                                    Int32 Re = 0;
                                    int row1 = startbit.GetUpperBound(0) + 1;
                                    int col1 = startbit.GetUpperBound(1) + 1;
                                    int row2 = length.GetUpperBound(0) + 1;
                                    int col2 = length.GetUpperBound(1) + 1;
                                    int row3 = method.GetUpperBound(0) + 1;
                                    int col3 = method.GetUpperBound(1) + 1;
                                    unsafe
                                    {
                                        fixed (Int32* fp1 = startbit)
                                        {
                                            fixed (Int32* fp2 = length)
                                            {
                                                fixed (Int32* fp3 = method)
                                                {
                                                    Int32*[] fstartbit = new Int32*[500];
                                                    Int32*[] flength = new Int32*[500];
                                                    Int32*[] fmethod = new Int32*[500];
                                                    for (int i = 0; i < row1; i++)
                                                    {
                                                        fstartbit[i] = fp1 + i * col1;
                                                    }
                                                    for (int i = 0; i < row2; i++)
                                                    {
                                                        flength[i] = fp2 + i * col2;
                                                    }
                                                    for (int i = 0; i < row3; i++)
                                                    {
                                                        fmethod[i] = fp3 + i * col3;
                                                    }
                                                    Re = A_J1939_DP.ANetT_Database_J1939_MsgAll(DUTName, ID, signal_N, fstartbit, flength, fmethod);
                                                }
                                            }
                                        }
                                    }
                                    String Temp = Re.ToString() + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                         Temp = Temp + ID[i].ToString() + "#";
                                        //Temp = Temp + "0x" + ID[i].ToString("x8").ToUpper() + "x" + "#";
                                    }
                                    Temp = Temp + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        Temp = Temp + signal_N[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        for (int j = 0; j < signal_N[i]; j++)
                                        {
                                            Temp = Temp + startbit[i, j].ToString() + "$";
                                        }
                                        Temp = Temp + "#";
                                    }
                                    Temp = Temp + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        for (int j = 0; j < signal_N[i]; j++)
                                        {
                                            Temp = Temp + length[i, j].ToString() + "$";
                                        }
                                        Temp = Temp + "#";
                                    }
                                    Temp = Temp + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        for (int j = 0; j < signal_N[i]; j++)
                                        {
                                            Temp = Temp + method[i, j].ToString() + "$";
                                        }
                                        Temp = Temp + "#";
                                    }
                                    Temp = Temp + "@";
                                    EnVariable_StateMachine_String.Value = Temp;
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_J1939_IDCheck
                            if (CAPL_FunctionName == "ANetT_Test_J1939_IDCheck")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    String StandardIDTemp = CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1);
                                    UInt32[] StandardID = new UInt32[500];
                                    Pos5 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (StandardIDTemp.IndexOf("#", Pos5 + 1) <= 0) break;
                                        //  StandardID[i] = Convert.ToUInt32(StandardIDTemp.Substring(Pos5 + 1, StandardIDTemp.IndexOf("#", Pos5 + 1) - Pos5 - 1));
                                        StandardID[i] = UInt32.Parse((StandardIDTemp.Substring(Pos5 + 1, StandardIDTemp.IndexOf("#", Pos5 + 1) - Pos5 - 1)), System.Globalization.NumberStyles.HexNumber);

                                        Pos5 = StandardIDTemp.IndexOf("#", Pos5 + 1);
                                    }
                                    string Describ = "";
                                    Int32 Re = A_J1939_DP.ANetT_Test_J1939_IDCheck(Path, Channel, StandardID, ref Describ);

                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describ + "@";

                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_J1939_DLCCheck
                            if (CAPL_FunctionName == "ANetT_Test_J1939_DLCCheck")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0 && Pos5 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    String StandardIDTemp = CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1);
                                    String StandardDLCTemp = CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1);
                                    UInt32[] StandardID = new UInt32[500];
                                    Pos6 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (StandardIDTemp.IndexOf("#", Pos6 + 1) <= 0) break;
                                        // StandardID[i] = UInt32.Parse(StandardIDTemp.Substring(Pos6 + 1, StandardIDTemp.IndexOf("#", Pos6 + 1) - Pos6 - 1)，System.Globalization.NumberStyles.HexNumber);
                                        StandardID[i] = UInt32.Parse((StandardIDTemp.Substring(Pos6 + 1, StandardIDTemp.IndexOf("#", Pos6 + 1) - Pos6 - 1)), System.Globalization.NumberStyles.HexNumber);

                                        Pos6 = StandardIDTemp.IndexOf("#", Pos6 + 1);
                                    }
                                    UInt32[] StandardDLC = new UInt32[500];
                                    Pos7 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (StandardDLCTemp.IndexOf("#", Pos7 + 1) <= 0) break;
                                        StandardDLC[i] = Convert.ToUInt32(StandardDLCTemp.Substring(Pos7 + 1, StandardDLCTemp.IndexOf("#", Pos7 + 1) - Pos7 - 1));
                                        Pos7 = StandardDLCTemp.IndexOf("#", Pos7 + 1);
                                    }
                                    string Describ = "";
                                    Int32 Re = A_J1939_DP.ANetT_Test_J1939_DLCCheck(Path, Channel, StandardID, StandardDLC, ref Describ);

                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describ + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_J1939_CycleCheck
                            if (CAPL_FunctionName == "ANetT_Test_J1939_CycleCheck")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0 && Pos5 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    String StandardIDTemp = CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1);
                                    String StandardCycleTimeTemp = CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1);
                                    UInt32[] StandardID = new UInt32[500];
                                    Pos6 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (StandardIDTemp.IndexOf("#", Pos6 + 1) <= 0) break;
                                        StandardID[i] = UInt32.Parse(StandardIDTemp.Substring(Pos6 + 1, StandardIDTemp.IndexOf("#", Pos6 + 1) - Pos6 - 1), System.Globalization.NumberStyles.HexNumber);
                                        Pos6 = StandardIDTemp.IndexOf("#", Pos6 + 1);
                                    }
                                    Int32[] StandardCycleTime = new Int32[500];
                                    Pos7 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (StandardCycleTimeTemp.IndexOf("#", Pos7 + 1) <= 0) break;
                                        StandardCycleTime[i] = Convert.ToInt32(StandardCycleTimeTemp.Substring(Pos7 + 1, StandardCycleTimeTemp.IndexOf("#", Pos7 + 1) - Pos7 - 1), 10);
                                        Pos7 = StandardCycleTimeTemp.IndexOf("#", Pos7 + 1);
                                    }
                                    string Describ = "";
                                    Int32 Re = A_J1939_DP.ANetT_Test_J1939_CycleCheck(Path, Channel, StandardID, StandardCycleTime, ref Describ);

                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describ + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_J1939_SamplingPoint
                            if (CAPL_FunctionName == "ANetT_Test_J1939_SamplingPoint")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    String Path = CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1);
                                    Int32 Re = A_J1939_DP.ANetT_Test_J1939_SamplingPoint(Channel, Path);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region DeleteIDArray
                            if (CAPL_FunctionName == "DeleteIDArray")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    String IDTemp = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    String deleteIDTemp = CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1);
                                    UInt32[] ID = new UInt32[500];
                                    Pos4 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (IDTemp.IndexOf("#", Pos4 + 1) <= 0) break;
                                        ID[i] = UInt32.Parse((IDTemp.Substring(Pos4 + 1, IDTemp.IndexOf("#", Pos4 + 1) - Pos4 - 1)), System.Globalization.NumberStyles.HexNumber);

                                        // ID[i] = UInt32.Parse(IDTemp.Substring(Pos4 + 1, IDTemp.IndexOf("#", Pos4 + 1) - Pos4 - 1));
                                        Pos4 = IDTemp.IndexOf("#", Pos4 + 1);
                                    }
                                    UInt32[] deleteID = new UInt32[500];
                                    Pos5 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (deleteIDTemp.IndexOf("#", Pos5 + 1) <= 0) break;
                                        deleteID[i] = UInt32.Parse(deleteIDTemp.Substring(Pos5 + 1, deleteIDTemp.IndexOf("#", Pos5 + 1) - Pos5 - 1), System.Globalization.NumberStyles.HexNumber);
                                        Pos5 = deleteIDTemp.IndexOf("#", Pos5 + 1);
                                    }
                                    Int32 Re = A_J1939_DP.DeleteIDArray(ID, deleteID);
                                    String Temp = Re.ToString() + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        Temp = Temp + ID[i].ToString() + "#";
                                        //Temp = Temp + "0x" + ID[i].ToString("x8").ToUpper() + "x" + "#";
                                    }
                                    Temp = Temp + "@";
                                    EnVariable_StateMachine_String.Value = Temp;
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion




                            #region ANetT_Test_J1939_Tbit
                            if (CAPL_FunctionName == "ANetT_Test_J1939_Tbit")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0)
                                {
                                    String Path1 = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    String Path2 = CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1);
                                    double Timebase = Convert.ToDouble(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    Int32 SampPoint = Convert.ToInt32(CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1));
                                    double CurSorA = 0;
                                    double CurSorB = 0;
                                    double Re = A_J1939_DP.ANetT_Test_J1939_Tbit(Path1, Path2, Timebase, SampPoint, ref CurSorA, ref CurSorB);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + CurSorA.ToString() + "@" + CurSorB.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_J1939_Symmetry
                            if (CAPL_FunctionName == "ANetT_Test_J1939_Symmetry")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    double Timebase = Convert.ToDouble(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    Int32 SampPoint = Convert.ToInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    double CurSorA = 0;
                                    double CurSorB = 0;
                                    double Re = A_J1939_DP.ANetT_Test_J1939_Symmetry(Path, Timebase, SampPoint, ref CurSorA, ref CurSorB);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + CurSorA.ToString() + "@" + CurSorB.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_J1939_InitialorFaultRecoverTime_OSC
                            if (CAPL_FunctionName == "ANetT_Test_J1939_InitialorFaultRecoverTime_OSC")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0 && Pos5 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    double Timebase = Convert.ToDouble(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    Int32 SampPoint = Convert.ToInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    double TrrggerV = Convert.ToDouble(CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1));
                                    double Re = A_J1939_DP.ANetT_Test_J1939_InitialorFaultRecoverTime_OSC(Path, Timebase, SampPoint, TrrggerV);
                                    Re = Re * 0.001;
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "s" + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion//返回的是ms
                            #region ANetT_Test_J1939_BusOffCheck
                            if (CAPL_FunctionName == "ANetT_Test_J1939_BusOffCheck")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    Int32 number = Convert.ToInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    string Describ = "";
                                    Int32 Re = A_J1939_DP.ANetT_Test_J1939_BusOffCheck(Path, Channel, number, ref Describ);

                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describ + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_J1939_Holdtime
                            if (CAPL_FunctionName == "ANetT_Test_J1939_Holdtime")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    double Timebase = Convert.ToDouble(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    Int32 SampPoint = Convert.ToInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    double Re = A_J1939_DP.ANetT_Test_J1939_Holdtime(Path, Timebase, SampPoint);
                                    Re = Re * 0.001;
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "s" + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion//返回的是ms
                            #region ANetT_Test_J1939_StuffBit
                            if (CAPL_FunctionName == "ANetT_Test_J1939_StuffBit")//路径@信号编码方式@通道@ID@信号个数@信号起始位@信号长度@@@
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);//路径
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);//信号编码方式
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);//通道
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);//ID
                                Pos6 = CAPL_String.IndexOf("@", Pos5 + 1);//信号个数
                                Pos7 = CAPL_String.IndexOf("@", Pos6 + 1);//信号起始位
                                Pos8 = CAPL_String.IndexOf("@", Pos7 + 1);//信号起长度

                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0 && Pos5 > 0 && Pos6 > 0 && Pos7 > 0 && Pos8 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 ByteOrder = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    String StandardIDTemp = CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1);
                                    String StandardDSignalnumTemp = CAPL_String.Substring(Pos5 + 1, Pos6 - Pos5 - 1);
                                    String StandardStartBitTemp = CAPL_String.Substring(Pos6 + 1, Pos7 - Pos6 - 1);
                                    String StandardLengthTemp = CAPL_String.Substring(Pos7 + 1, Pos8 - Pos7 - 1);
                                    UInt32[] StandardID = new UInt32[500];
                                    UInt32[] StandardSignalnum = new UInt32[500];
                                    UInt32[,] StandardStartBit = new UInt32[200, 500];
                                    UInt32[,] StandardLength = new UInt32[200, 500];

                                    Pos9 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (StandardIDTemp.IndexOf("#", Pos9 + 1) <= 0) break;
                                        StandardID[i] = UInt32.Parse((StandardIDTemp.Substring(Pos9 + 1, StandardIDTemp.IndexOf("#", Pos9 + 1) - Pos9 - 1)), System.Globalization.NumberStyles.HexNumber);
                                        Pos9 = StandardIDTemp.IndexOf("#", Pos9 + 1);
                                    }
                                    Pos10 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (StandardDSignalnumTemp.IndexOf("#", Pos10 + 1) <= 0) break;
                                        StandardSignalnum[i] = Convert.ToUInt32(StandardDSignalnumTemp.Substring(Pos10 + 1, StandardDSignalnumTemp.IndexOf("#", Pos10 + 1) - Pos10 - 1));
                                        //  StandardSignalnum[i] = UInt32.Parse((StandardDSignalnumTemp.Substring(Pos10 + 1, StandardDSignalnumTemp.IndexOf("#", Pos10 + 1) - Pos10 - 1)), System.Globalization.NumberStyles.HexNumber);
                                        Pos10 = StandardDSignalnumTemp.IndexOf("#", Pos10 + 1);
                                    }
                                    Pos11 = -1;
                                    for (int i = 0; StandardID[i] != 0; i++)
                                    {
                                        for (int j = 0; j < StandardSignalnum[i]; j++)
                                        {
                                            if (StandardStartBitTemp.IndexOf("#", Pos11 + 1) <= 0) break;
                                            StandardStartBit[i, j] = Convert.ToUInt32(StandardStartBitTemp.Substring(Pos11 + 1, StandardStartBitTemp.IndexOf("#", Pos11 + 1) - Pos11 - 1));
                                            //  StandardStartBit[i, j] = UInt32.Parse((StandardStartBitTemp.Substring(Pos11 + 1, StandardStartBitTemp.IndexOf("#", Pos11 + 1) - Pos11 - 1)), System.Globalization.NumberStyles.HexNumber);
                                            Pos11 = StandardStartBitTemp.IndexOf("#", Pos11 + 1);
                                        }
                                    }
                                    Pos12 = -1;
                                    for (int i = 0; StandardID[i] != 0; i++)
                                    {
                                        for (int j = 0; j < StandardSignalnum[i]; j++)
                                        {
                                            if (StandardLengthTemp.IndexOf("#", Pos12 + 1) <= 0) break;
                                            StandardLength[i, j] = Convert.ToUInt32(StandardLengthTemp.Substring(Pos12 + 1, StandardLengthTemp.IndexOf("#", Pos12 + 1) - Pos12 - 1));
                                            // StandardLength[i, j] = UInt32.Parse((StandardLengthTemp.Substring(Pos12 + 1, StandardLengthTemp.IndexOf("#", Pos12 + 1) - Pos12 - 1)), System.Globalization.NumberStyles.HexNumber);
                                            Pos12 = StandardLengthTemp.IndexOf("#", Pos12 + 1);
                                        }
                                    }

                                    string Describ = "";
                                    Int32 Re = A_J1939_DP.ANetT_Test_J1939_StuffBit(Path, ByteOrder, Channel, StandardID, StandardSignalnum, StandardStartBit, StandardLength, ref Describ);

                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describ + "@";

                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region Delete_RecordFile
                            if (CAPL_FunctionName == "Delete_RecordFile")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 Re = A_J1939_DP.Delete_RecordFile(Path);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_J1939_MessageRepeatCheck
                            if (CAPL_FunctionName == "ANetT_Test_J1939_MessageRepeatCheck")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    String StandardIDTemp = CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1);
                                    UInt32 ID = Convert.ToUInt32(StandardIDTemp, 16);
                                    string Describ = "";
                                    Int32 Re = A_J1939_DP.ANetT_Test_J1939_MessageRepeatCheck(Path, Channel, ID, ref Describ);

                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describ + "@";

                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion

                            //LIN_DUT_DataProcessW状态机主体
                            #region ANetT_Database_LIN_File
                            if (CAPL_FunctionName == "ANetT_Database_LIN_File")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 Re = A_LD_DP.ANetT_Database_LIN_File(Path);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Database_LINID
                            if (CAPL_FunctionName == "ANetT_Database_LINID")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    String DUTName = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    UInt32[] ID = new UInt32[500];
                                    UInt32[] DLC = new UInt32[500];
                                    Int32 Re = A_LD_DP.ANetT_Database_LINID(DUTName, ID, DLC);
                                    String Temp = Re.ToString() + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        //Temp = Temp +"0x"+ Convert.ToString(ID[i],16) + "#";
                                        Temp = Temp + ID[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        Temp = Temp + DLC[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";
                                    EnVariable_StateMachine_String.Value = Temp;
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Database_LINSignal
                            if (CAPL_FunctionName == "ANetT_Database_LINSignal")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    String DUTName = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    UInt32[] ID = new UInt32[100];
                                    UInt32[] Singal_N = new UInt32[100];
                                    UInt32[,] Startbit = new UInt32[100, 200];
                                    UInt32[,] Length = new UInt32[100, 200];
                                    Int32 Re = A_LD_DP.ANetT_Database_LINSignal(DUTName, ID, Singal_N, Startbit, Length);
                                    string Temp = Re.ToString() + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        //Temp = Temp + "0x" + Convert.ToString(ID[i], 16) + "#";
                                        Temp = Temp + ID[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        Temp = Temp + Singal_N[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        for (int j = 0; j < Singal_N[i]; j++)
                                        {
                                            Temp = Temp + Startbit[i, j].ToString() + "#";
                                        }
                                    }
                                    Temp = Temp + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        for (int j = 0; j < Singal_N[i]; j++)
                                        {
                                            Temp = Temp + Length[i, j].ToString() + "#";
                                        }
                                    }
                                    Temp = Temp + "@";
                                    //string Temp = "该节点所含报文个数及ID：" + Re.ToString() + "\r\n";
                                    //for (int i = 0; i < Re; i++)
                                    //{
                                    //    Int32 n = i + 1;
                                    //    Temp = Temp + "第" + n + "个报文的信号信息：" + "报文ID：";
                                    //    Temp = Temp +"0x"+ Convert.ToString(ID[i],16) + "信号个数：";
                                    //    Temp = Temp + Singal_N[i].ToString() + "\r\n";
                                    //    Temp = Temp + "起始位#信号长度" + "\r\n";
                                    //    UInt32 tt = Singal_N[i + 1];
                                    //    for (int j = 0; j < Singal_N[i]; j++)
                                    //    {
                                    //            Temp = Temp + Startbit[i,j].ToString() + "#";
                                    //            Temp = Temp + Length[i,j].ToString() + "\r\n";
                                    //    }
                                    //}
                                    //Temp = Temp + "@";
                                    EnVariable_StateMachine_String.Value = Temp;
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Database_LINResponseError
                            if (CAPL_FunctionName == "ANetT_Database_LINResponseError")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    String DUTName = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    UInt32[] ID = new UInt32[100];
                                    UInt32[] Startbit = new UInt32[100];
                                    Int32 Re = A_LD_DP.ANetT_Database_LINResponseError(DUTName, ID, Startbit);
                                    string Temp = Re.ToString() + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        //Temp = Temp + "0x" + Convert.ToString(ID[i], 16) + "#";
                                        Temp = Temp + ID[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        Temp = Temp + Startbit[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";
                                    //string Temp = "该节点所含报文个数：" + Re.ToString() + "\r\n";
                                    //for (int i = 0; i < Re; i++)
                                    //{
                                    //    Int32 n = i + 1;
                                    //    Temp = Temp + "第" + n + "个报文的ResponseError信息：";
                                    //    Temp = Temp + "ID:" + "0x" + Convert.ToString(ID[i], 16) + "；";
                                    //    Temp = Temp + "#起始位：" + Startbit[i].ToString() + "\r\n";
                                    //}
                                    //Temp = Temp + "@";

                                    EnVariable_StateMachine_String.Value = Temp;
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Database_LINSchedule
                            if (CAPL_FunctionName == "ANetT_Database_LINSchedule")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 == -1)
                                {
                                    UInt32[] ST = new UInt32[100];
                                    UInt32[] STDelay = new UInt32[100];
                                    UInt32[] DLC = new UInt32[500];
                                    Int32 Re = A_LD_DP.ANetT_Database_LINSchedule(ST, STDelay, DLC);
                                    string Temp = Re.ToString() + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        //Temp = Temp + "0x" + Convert.ToString(ID[i], 16) + "#";
                                        Temp = Temp + ST[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        //Temp = Temp + "0x" + Convert.ToString(ID[i], 16) + "#";
                                        Temp = Temp + STDelay[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";
                                    for (int i = 0; i < Re; i++)
                                    {
                                        //Temp = Temp + "0x" + Convert.ToString(ID[i], 16) + "#";
                                        Temp = Temp + DLC[i].ToString() + "#";
                                    }
                                    Temp = Temp + "@";
                                    //string Temp = "调度表所含报文个数：" + Re.ToString() + "\r\n";
                                    //for (int i = 0; i < Re; i++)
                                    //{
                                    //    Temp = Temp + "0x"+Convert.ToString(ST[i],16) + "#" + STDelay[i]+"ms" + "\r\n";
                                    //}
                                    //Temp = Temp + "@";
                                    EnVariable_StateMachine_String.Value = Temp;
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_LINDLC
                            if (CAPL_FunctionName == "ANetT_Test_LINDLC")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    String StandardIDTemp = CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1);
                                    String StandardDLCTemp = CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1);
                                    UInt32[] StandardID = new UInt32[500];
                                    Pos6 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (StandardIDTemp.IndexOf("#", Pos6 + 1) <= 0) break;
                                        // StandardID[i] = UInt32.Parse(StandardIDTemp.Substring(Pos6 + 1, StandardIDTemp.IndexOf("#", Pos6 + 1) - Pos6 - 1)，System.Globalization.NumberStyles.HexNumber);
                                        //StandardID[i]= UInt32.Parse((StandardIDTemp.Substring(Pos6 + 1, StandardIDTemp.IndexOf("#", Pos6 + 1) - Pos6 - 1)), System.Globalization.NumberStyles.HexNumber);
                                        StandardID[i] = UInt32.Parse(StandardIDTemp.Substring(Pos6 + 1, StandardIDTemp.IndexOf("#", Pos6 + 1) - Pos6 - 1), System.Globalization.NumberStyles.HexNumber);
                                        Pos6 = StandardIDTemp.IndexOf("#", Pos6 + 1);
                                    }
                                    UInt32[] StandardDLC = new UInt32[500];
                                    Pos7 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (StandardDLCTemp.IndexOf("#", Pos7 + 1) <= 0) break;
                                        StandardDLC[i] = Convert.ToUInt32(StandardDLCTemp.Substring(Pos7 + 1, StandardDLCTemp.IndexOf("#", Pos7 + 1) - Pos7 - 1));
                                        Pos7 = StandardDLCTemp.IndexOf("#", Pos7 + 1);
                                    }
                                    string Describ = "";
                                    Int32 Re = A_LD_DP.ANetT_Test_LINDLC(Path, StandardID, StandardDLC, ref  Describ);

                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describ + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_LIN_StuffBit
                            if (CAPL_FunctionName == "ANetT_Test_LIN_StuffBit")//路径@ID@信号个数@信号起始位@信号长度@@@
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);//路径
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);//ID
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);//信号个数
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);//信号起始位
                                Pos6 = CAPL_String.IndexOf("@", Pos5 + 1);//信号起长度
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0 && Pos5 > 0 && Pos6 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    String StandardIDTemp = CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1);
                                    String StandardDSignalnumTemp = CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1);
                                    String StandardStartBitTemp = CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1);
                                    String StandardLengthTemp = CAPL_String.Substring(Pos5 + 1, Pos6 - Pos5 - 1);
                                    UInt32[] StandardID = new UInt32[500];
                                    UInt32[] StandardSignalnum = new UInt32[500];
                                    UInt32[,] StandardStartBit = new UInt32[200, 500];
                                    UInt32[,] StandardLength = new UInt32[200, 500];

                                    Pos9 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (StandardIDTemp.IndexOf("#", Pos9 + 1) <= 0) break;
                                        StandardID[i] = UInt32.Parse((StandardIDTemp.Substring(Pos9 + 1, StandardIDTemp.IndexOf("#", Pos9 + 1) - Pos9 - 1)), System.Globalization.NumberStyles.HexNumber);
                                        //StandardID[i] = Convert.ToUInt32(StandardIDTemp.Substring(Pos9 + 1, StandardIDTemp.IndexOf("#", Pos9 + 1) - Pos9 - 1));
                                        Pos9 = StandardIDTemp.IndexOf("#", Pos9 + 1);
                                    }
                                    Pos10 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (StandardDSignalnumTemp.IndexOf("#", Pos10 + 1) <= 0) break;
                                        StandardSignalnum[i] = Convert.ToUInt32(StandardDSignalnumTemp.Substring(Pos10 + 1, StandardDSignalnumTemp.IndexOf("#", Pos10 + 1) - Pos10 - 1));
                                        Pos10 = StandardDSignalnumTemp.IndexOf("#", Pos10 + 1);
                                    }
                                    Pos11 = -1;
                                    for (int i = 0; StandardID[i] != 0; i++)
                                    {
                                        for (int j = 0; j < StandardSignalnum[i]; j++)
                                        {
                                            if (StandardStartBitTemp.IndexOf("#", Pos11 + 1) <= 0) break;
                                            StandardStartBit[i, j] = Convert.ToUInt32(StandardStartBitTemp.Substring(Pos11 + 1, StandardStartBitTemp.IndexOf("#", Pos11 + 1) - Pos11 - 1));
                                            Pos11 = StandardStartBitTemp.IndexOf("#", Pos11 + 1);
                                        }
                                    }
                                    Pos12 = -1;
                                    for (int i = 0; StandardID[i] != 0; i++)
                                    {
                                        for (int j = 0; j < StandardSignalnum[i]; j++)
                                        {
                                            if (StandardLengthTemp.IndexOf("#", Pos12 + 1) <= 0) break;
                                            StandardLength[i, j] = Convert.ToUInt32(StandardLengthTemp.Substring(Pos12 + 1, StandardLengthTemp.IndexOf("#", Pos12 + 1) - Pos12 - 1));
                                            Pos12 = StandardLengthTemp.IndexOf("#", Pos12 + 1);
                                        }
                                    }

                                    string Describe = "";
                                    Int32 Re = A_LD_DP.ANetT_Test_LIN_StuffBit(Path, StandardID, StandardSignalnum, StandardStartBit, StandardLength, ref Describe);

                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describe + "@";

                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_LINChecksum_Enhanced
                            if (CAPL_FunctionName == "ANetT_Test_LINChecksum_Enhanced")//路径@信号编码方式@通道@ID@信号个数@信号起始位@信号长度@@@
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);//路径
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);//ID 


                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    String checkPIDTemp = CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1);
                                    UInt32[] checkPID = new UInt32[500];

                                    Pos9 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (checkPIDTemp.IndexOf("#", Pos9 + 1) <= 0) break;
                                        checkPID[i] = UInt32.Parse((checkPIDTemp.Substring(Pos9 + 1, checkPIDTemp.IndexOf("#", Pos9 + 1) - Pos9 - 1)), System.Globalization.NumberStyles.HexNumber);
                                        Pos9 = checkPIDTemp.IndexOf("#", Pos9 + 1);
                                    }
                                    string Describe = "";
                                    Int32 Re = A_LD_DP.ANetT_Test_LINChecksum_Enhanced(Path, checkPID, ref Describe);

                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describe + "@";

                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_LINScheduleSeq
                            if (CAPL_FunctionName == "ANetT_Test_LINScheduleSeq")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    String STTemp = CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1);
                                    UInt32[] ST = new UInt32[500];
                                    Pos6 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (STTemp.IndexOf("#", Pos6 + 1) <= 0) break;
                                        // StandardID[i] = UInt32.Parse(StandardIDTemp.Substring(Pos6 + 1, StandardIDTemp.IndexOf("#", Pos6 + 1) - Pos6 - 1)，System.Globalization.NumberStyles.HexNumber);
                                        //StandardID[i]= UInt32.Parse((StandardIDTemp.Substring(Pos6 + 1, StandardIDTemp.IndexOf("#", Pos6 + 1) - Pos6 - 1)), System.Globalization.NumberStyles.HexNumber);
                                        ST[i] = UInt32.Parse(STTemp.Substring(Pos6 + 1, STTemp.IndexOf("#", Pos6 + 1) - Pos6 - 1), System.Globalization.NumberStyles.HexNumber);
                                        Pos6 = STTemp.IndexOf("#", Pos6 + 1);
                                    }

                                    string Describe = "";
                                    Int32 Re = A_LD_DP.ANetT_Test_LINScheduleSeq(Path, ST, ref  Describe);

                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describe + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_LINScheduleSeqTime
                            if (CAPL_FunctionName == "ANetT_Test_LINScheduleSeqTime")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    String STTemp = CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1);
                                    String STDelayTemp = CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1);
                                    UInt32[] ST = new UInt32[500];
                                    UInt32[] STDelay = new UInt32[500];
                                    Pos6 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (STTemp.IndexOf("#", Pos6 + 1) <= 0) break;
                                        // StandardID[i] = UInt32.Parse(StandardIDTemp.Substring(Pos6 + 1, StandardIDTemp.IndexOf("#", Pos6 + 1) - Pos6 - 1)，System.Globalization.NumberStyles.HexNumber);
                                        //StandardID[i]= UInt32.Parse((StandardIDTemp.Substring(Pos6 + 1, StandardIDTemp.IndexOf("#", Pos6 + 1) - Pos6 - 1)), System.Globalization.NumberStyles.HexNumber);
                                        ST[i] = UInt32.Parse(STTemp.Substring(Pos6 + 1, STTemp.IndexOf("#", Pos6 + 1) - Pos6 - 1), System.Globalization.NumberStyles.HexNumber);
                                        Pos6 = STTemp.IndexOf("#", Pos6 + 1);
                                    }
                                    Pos7 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (STDelayTemp.IndexOf("#", Pos7 + 1) <= 0) break;
                                        // StandardID[i] = UInt32.Parse(StandardIDTemp.Substring(Pos6 + 1, StandardIDTemp.IndexOf("#", Pos6 + 1) - Pos6 - 1)，System.Globalization.NumberStyles.HexNumber);
                                        //StandardID[i]= UInt32.Parse((StandardIDTemp.Substring(Pos6 + 1, StandardIDTemp.IndexOf("#", Pos6 + 1) - Pos6 - 1)), System.Globalization.NumberStyles.HexNumber);
                                        STDelay[i] = Convert.ToUInt32(STDelayTemp.Substring(Pos7 + 1, STDelayTemp.IndexOf("#", Pos7 + 1) - Pos7 - 1));
                                        Pos7 = STDelayTemp.IndexOf("#", Pos7 + 1);
                                    }
                                    string Describe = "";
                                    Int32 Re = A_LD_DP.ANetT_Test_LINScheduleSeqTime(Path, ST, STDelay, ref  Describe);

                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describe + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_LINResponseError
                            if (CAPL_FunctionName == "ANetT_Test_LINResponseError")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);//路径
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);//ID
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);//信号起始位
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    String StandardIDTemp = CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1);
                                    String StandardStartBitTemp = CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1);
                                    UInt32[] ID = new UInt32[100];
                                    UInt32[] StartBit = new UInt32[100];

                                    Pos9 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (StandardIDTemp.IndexOf("#", Pos9 + 1) <= 0) break;
                                        ID[i] = UInt32.Parse((StandardIDTemp.Substring(Pos9 + 1, StandardIDTemp.IndexOf("#", Pos9 + 1) - Pos9 - 1)), System.Globalization.NumberStyles.HexNumber);
                                        //StandardID[i] = Convert.ToUInt32(StandardIDTemp.Substring(Pos9 + 1, StandardIDTemp.IndexOf("#", Pos9 + 1) - Pos9 - 1));
                                        Pos9 = StandardIDTemp.IndexOf("#", Pos9 + 1);
                                    }
                                    Pos10 = -1;
                                    for (int i = 0; i < 500; i++)
                                    {
                                        if (StandardStartBitTemp.IndexOf("#", Pos10 + 1) <= 0) break;
                                        StartBit[i] = Convert.ToUInt32(StandardStartBitTemp.Substring(Pos10 + 1, StandardStartBitTemp.IndexOf("#", Pos10 + 1) - Pos10 - 1));
                                        Pos10 = StandardStartBitTemp.IndexOf("#", Pos10 + 1);
                                    }
                                    string Describe = "";
                                    Int32 Re = A_LD_DP.ANetT_Test_LINResponseError(Path, ID, StartBit, ref Describe);

                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describe + "@";

                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_LINBitLevel
                            if (CAPL_FunctionName == "ANetT_Test_LINBitLevel")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);
                                Pos6 = CAPL_String.IndexOf("@", Pos5 + 1);
                                Pos7 = CAPL_String.IndexOf("@", Pos6 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0 && Pos5 > 0 && Pos6 > 0 && Pos7 > 0)
                                {
                                    double CursorA = 0;
                                    double CursorB = 0;
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    UInt32 Mode = Convert.ToUInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    double Timebase = Convert.ToDouble(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    double Baudrate = Convert.ToDouble(CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1));
                                    UInt32 SampPoint = Convert.ToUInt32(CAPL_String.Substring(Pos5 + 1, Pos6 - Pos5 - 1));
                                    UInt32 Level = Convert.ToUInt32(CAPL_String.Substring(Pos6 + 1, Pos7 - Pos6 - 1));
                                    string Describe = "";
                                    Int32 Re = A_LD_DP.ANetT_Test_LINBitLevel(Path, Mode, Timebase, Baudrate, SampPoint, Level, ref CursorA, ref CursorB);
                                    Describe = CursorA + "@" + CursorB;
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describe + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_LINDutyRatio
                            if (CAPL_FunctionName == "ANetT_Test_LINDutyRatio")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);
                                Pos6 = CAPL_String.IndexOf("@", Pos5 + 1);
                                Pos7 = CAPL_String.IndexOf("@", Pos6 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0 && Pos5 > 0 && Pos6 > 0 && Pos7 > 0)
                                {
                                    double MinVa = 0;
                                    double MinVb = 0;
                                    double MaxVa = 0;
                                    double MaxVb = 0;
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    UInt32 Mode = Convert.ToUInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    double Voltage = Convert.ToDouble(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    UInt32 SampPoint = Convert.ToUInt32(CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1));
                                    double Timebase = Convert.ToDouble(CAPL_String.Substring(Pos5 + 1, Pos6 - Pos5 - 1));
                                    double Baudrate = Convert.ToDouble(CAPL_String.Substring(Pos6 + 1, Pos7 - Pos6 - 1));
                                    string Describe = "";
                                    Int32 Re = A_LD_DP.ANetT_Test_LINDutyRatio(Path, Mode, Voltage, SampPoint, Timebase, Baudrate, ref  MinVa, ref  MinVb, ref  MaxVa, ref  MaxVb);
                                    Describe = MinVa + "@" + MinVb + "@" + MaxVa + "@" + MaxVb;
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describe + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_LINBaudrate
                            if (CAPL_FunctionName == "ANetT_Test_LINBaudrate")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);
                                Pos6 = CAPL_String.IndexOf("@", Pos5 + 1);
                                Pos7 = CAPL_String.IndexOf("@", Pos6 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0 && Pos5 > 0 && Pos6 > 0 && Pos7 > 0)
                                {
                                    double CursorA = 0;
                                    double CursorB = 0;
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    UInt32 Mode = Convert.ToUInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    double Voltage = Convert.ToDouble(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    double Timebase = Convert.ToDouble(CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1));
                                    UInt32 SampPoint = Convert.ToUInt32(CAPL_String.Substring(Pos5 + 1, Pos6 - Pos5 - 1));
                                    double Baudrate = Convert.ToDouble(CAPL_String.Substring(Pos6 + 1, Pos7 - Pos6 - 1));
                                    string Describe = "";
                                    Int32 Re = A_LD_DP.ANetT_Test_LINBaudrate(Path, Mode, Voltage, Timebase, SampPoint, Baudrate, ref CursorA, ref CursorB);
                                    Describe = CursorA + "@" + CursorB;
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describe + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region ANetT_Test_LINlength
                            if (CAPL_FunctionName == "ANetT_Test_LINlength")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);
                                Pos6 = CAPL_String.IndexOf("@", Pos5 + 1);
                                Pos7 = CAPL_String.IndexOf("@", Pos6 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0 && Pos5 > 0 && Pos6 > 0 && Pos7 > 0)
                                {
                                    double CursorA = 0;
                                    double CursorB = 0;
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    UInt32 Mode = Convert.ToUInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    double Voltage = Convert.ToDouble(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    double Timebase = Convert.ToDouble(CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1));
                                    UInt32 SampPoint = Convert.ToUInt32(CAPL_String.Substring(Pos5 + 1, Pos6 - Pos5 - 1));
                                    double Baudrate = Convert.ToDouble(CAPL_String.Substring(Pos6 + 1, Pos7 - Pos6 - 1));
                                    string Describe = "";
                                    Int32 Re = A_LD_DP.ANetT_Test_LINlength(Path, Mode, Voltage, Timebase, SampPoint, Baudrate, ref CursorA, ref CursorB);
                                    Describe = CursorA + "@" + CursorB;
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@" + Describe + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion

                            //MDO4034C状态机主体
                            #region OSC_Set
                            if (CAPL_FunctionName == "OSC_Set")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    String IP = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    UInt32 Re = OSC_Set(IP);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_Initial
                            if (CAPL_FunctionName == "OSC_Initial")
                            {
                                UInt32 Re = OSC_Initial();
                                EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                StateMachine_TIME = DateTime.Now;
                                EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                            }
                            #endregion
                            #region OSC_AutoSet
                            if (CAPL_FunctionName == "OSC_AutoSet")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    Int32 x = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    UInt32 Re = OSC_AutoSet(x);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_EnablePanel
                            if (CAPL_FunctionName == "OSC_EnablePanel")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    Int32 x = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    UInt32 Re = OSC_EnablePanel(x);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_RunStop
                            if (CAPL_FunctionName == "OSC_RunStop")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    Int32 x = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    UInt32 Re = OSC_RunStop(x);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_RunStopSequence
                            if (CAPL_FunctionName == "OSC_RunStopSequence")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    Int32 x = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    UInt32 Re = OSC_RunStopSequence(x);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_TriggerModel
                            if (CAPL_FunctionName == "OSC_TriggerModel")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    Int32 x = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    UInt32 Re = OSC_TriggerModel(x);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_EdgeTrigger
                            if (CAPL_FunctionName == "OSC_EdgeTrigger")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0 && Pos5 > 0)
                                {
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    double Level = Convert.ToDouble(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    Int32 Mode = Convert.ToInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    Int32 Coupl = Convert.ToInt32(CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1));
                                    UInt32 Re = OSC_EdgeTrigger(Channel, Level, Mode, Coupl);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_PulseWidthTrigger
                            if (CAPL_FunctionName == "OSC_PulseWidthTrigger")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);
                                Pos6 = CAPL_String.IndexOf("@", Pos5 + 1);
                                Pos7 = CAPL_String.IndexOf("@", Pos6 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0 && Pos5 > 0 && Pos6 > 0 && Pos7 > 0)
                                {
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    double Level = Convert.ToDouble(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    Int32 Mode = Convert.ToInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    double parm1 = Convert.ToDouble(CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1));
                                    double parm2 = Convert.ToDouble(CAPL_String.Substring(Pos5 + 1, Pos6 - Pos5 - 1));
                                    Int32 polar = Convert.ToInt32(CAPL_String.Substring(Pos6 + 1, Pos7 - Pos6 - 1));
                                    UInt32 Re = OSC_PulseWidthTrigger(Channel, Level, Mode, parm1, parm2, polar);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_CANBitRateConfig
                            if (CAPL_FunctionName == "OSC_CANBitRateConfig")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    Int32 B = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    Int32 Baudrate = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    UInt32 Re = OSC_CANBitRateConfig(B, Baudrate);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_CANTriggerTypeFrame
                                    if (CAPL_FunctionName == "OSC_CANTriggerTypeFrame")
                                    {
                                        Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                        Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                        Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                        if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0)
                                        {
                                            Int32 B = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                            Int32 type = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                            Int32 FrameType = Convert.ToInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                            UInt32 Re = OSC_CANTriggerTypeFrame(B, type, FrameType);
                                            EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                            StateMachine_TIME = DateTime.Now;
                                            EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                        }
                                        else
                                        {
                                            EnVariable_StateMachine_String.Value = "InputError";
                                            StateMachine_TIME = DateTime.Now;
                                            EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                        }
                                    }
                            #endregion

                            /* #region OSC_CANTriggerTypeFrame
                            if (CAPL_FunctionName == "OSC_CANTriggerTypeFrame")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);

                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    Int32 B = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    Int32 FrameType = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    UInt32 Re = OSC_CANTriggerTypeFrame(B, FrameType);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            */
                            #region OSC_CANTriggerIDDataFrame
                            if (CAPL_FunctionName == "OSC_CANTriggerIDDataFrame")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);
                                Pos6 = CAPL_String.IndexOf("@", Pos5 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0 && Pos5 > 0 && Pos6 > 0)
                                {
                                    Int32 B = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    Int32 Mode = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    UInt32 ID = Convert.ToUInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1), 16);
                                    Int32 DataSize = Convert.ToInt32(CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1));
                                    UInt64 DataValue = Convert.ToUInt64(CAPL_String.Substring(Pos5 + 1, Pos6 - Pos5 - 1));
                                    UInt32 Re = OSC_CANTriggerIDDataFrame(B, Mode, ID, DataSize, DataValue);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_CANTriggerDataFrame
                            if (CAPL_FunctionName == "OSC_CANTriggerDataFrame")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0)
                                {
                                    Int32 B = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    Int32 DataSize = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    UInt64 DataValue = Convert.ToUInt64(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    UInt32 Re = OSC_CANTriggerDataFrame(B, DataSize, DataValue);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_CANTriggerID
                            if (CAPL_FunctionName == "OSC_CANTriggerID")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0)
                                {
                                    Int32 B = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    Int32 Mode = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    UInt32 ID = Convert.ToUInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1), 16);
                                    UInt32 Re = OSC_CANTriggerID(B, Mode, ID);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_CANConfig
                            if (CAPL_FunctionName == "OSC_CANConfig")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);
                                Pos6 = CAPL_String.IndexOf("@", Pos5 + 1);
                                Pos7 = CAPL_String.IndexOf("@", Pos6 + 1);
                                Pos8 = CAPL_String.IndexOf("@", Pos7 + 1);
                                Pos9 = CAPL_String.IndexOf("@", Pos8 + 1);
                                Pos10 = CAPL_String.IndexOf("@", Pos9 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0 && Pos5 > 0 && Pos6 > 0 && Pos7 > 0 && Pos8 > 0 && Pos9 > 0 && Pos10 > 0)
                                {
                                    Int32 B = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    Int32 Source = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    Int32 BitRate = Convert.ToInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    Int32 SampPoint = Convert.ToInt32(CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1));
                                    Int32 DisplayFormat = Convert.ToInt32(CAPL_String.Substring(Pos5 + 1, Pos6 - Pos5 - 1));
                                    Int32 DisplayType = Convert.ToInt32(CAPL_String.Substring(Pos6 + 1, Pos7 - Pos6 - 1));
                                    Int32 Position = Convert.ToInt32(CAPL_String.Substring(Pos7 + 1, Pos8 - Pos7 - 1));
                                    double TrigLevel = Convert.ToDouble(CAPL_String.Substring(Pos8 + 1, Pos9 - Pos8 - 1));
                                    Int32 signaltype = Convert.ToInt32(CAPL_String.Substring(Pos9 + 1, Pos10 - Pos9 - 1));
                                    UInt32 Re = OSC_CANConfig(B, Source, BitRate, SampPoint, DisplayFormat, DisplayType, Position, TrigLevel, signaltype);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_BusDisplay
                            if (CAPL_FunctionName == "OSC_BusDisplay")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    Int32 B = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    Int32 Display = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    UInt32 Re = OSC_BusDisplay(B, Display);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_Single
                            if (CAPL_FunctionName == "OSC_Single")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    Int32 Mode = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    UInt32 Re = OSC_Single(Mode);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_Cursor
                            if (CAPL_FunctionName == "OSC_Cursor")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);
                                Pos6 = CAPL_String.IndexOf("@", Pos5 + 1);
                                Pos7 = CAPL_String.IndexOf("@", Pos6 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0 && Pos5 > 0 && Pos6 > 0 && Pos7 > 0)
                                {
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    Int32 Mode = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    double Xa = Convert.ToDouble(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    double Xb = Convert.ToDouble(CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1));
                                    double Ya = Convert.ToDouble(CAPL_String.Substring(Pos5 + 1, Pos6 - Pos5 - 1));
                                    double Yb = Convert.ToDouble(CAPL_String.Substring(Pos6 + 1, Pos7 - Pos6 - 1));
                                    UInt32 Re = OSC_Cursor(Channel, Mode, Xa, Xb, Ya, Yb);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_CursorLink
                            if (CAPL_FunctionName == "OSC_CursorLink")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    Int32 Select = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    UInt32 Re = OSC_CursorLink(Select);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_MeasurClear
                            if (CAPL_FunctionName == "OSC_MeasurClear")
                            {
                                UInt32 Re = OSC_MeasurClear();
                                EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                StateMachine_TIME = DateTime.Now;
                                EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                            }
                            #endregion
                            #region OSC_MeasurAdd
                            if (CAPL_FunctionName == "OSC_MeasurAdd")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    Int32 Select = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    double Re = OSC_MeasurAdd(Channel, Select);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_MeasurRise
                            if (CAPL_FunctionName == "OSC_MeasurRise")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    double Re = OSC_MeasurRise(Channel);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_MeasurFall
                            if (CAPL_FunctionName == "OSC_MeasurFall")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    double Re = OSC_MeasurFall(Channel);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_TimeBase
                            if (CAPL_FunctionName == "OSC_TimeBase")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0)
                                {
                                    double Timebase = Convert.ToDouble(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    Int32 Position = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    Int32 RecordLength = Convert.ToInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    UInt32 Re = OSC_TimeBase(Timebase, Position, RecordLength);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_AcquireWave
                            if (CAPL_FunctionName == "OSC_AcquireWave")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    String Path = CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1);
                                    UInt32 Re = OSC_AcquireWave(Channel, Path);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_SaveImage
                            if (CAPL_FunctionName == "OSC_SaveImage")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    String Path = CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                    Int32 Mode = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    UInt32 Re = OSC_SaveImage(Path, Mode);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_MathOn
                            if (CAPL_FunctionName == "OSC_MathOn")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0)
                                {
                                    Int32 channel1 = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    Int32 channel2 = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    Int32 math = Convert.ToInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    UInt32 Re = OSC_MathOn(channel1, channel2, math);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_MathOff
                            if (CAPL_FunctionName == "OSC_MathOff")
                            {
                                UInt32 Re = OSC_MathOff();
                                EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                StateMachine_TIME = DateTime.Now;
                                EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                            }
                            #endregion
                            #region OSC_ChannelDisplay
                            if (CAPL_FunctionName == "OSC_ChannelDisplay")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    Int32 Display = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    UInt32 Re = OSC_ChannelDisplay(Channel, Display);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_LINTrigID
                            if (CAPL_FunctionName == "OSC_LINTrigID")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    Int32 B = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    UInt32 ID = Convert.ToUInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1), 16);
                                    UInt32 Re = OSC_LINTrigID(B, ID);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_LINTrigDataFrame
                            if (CAPL_FunctionName == "OSC_LINTrigDataFrame")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0)
                                {
                                    Int32 B = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    Int32 DataSize = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    UInt64 DataValue = Convert.ToUInt64(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    UInt32 Re = OSC_LINTrigDataFrame(B, DataSize, DataValue);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_LINTrigIDDataFrame
                            if (CAPL_FunctionName == "OSC_LINTrigIDDataFrame")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0 && Pos5 > 0)
                                {
                                    Int32 B = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    UInt32 ID = Convert.ToUInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1), 16);
                                    Int32 DataSize = Convert.ToInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    UInt64 DataValue = Convert.ToUInt64(CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1));
                                    UInt32 Re = OSC_LINTrigIDDataFrame(B, ID, DataSize, DataValue);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_LINTrigError
                            if (CAPL_FunctionName == "OSC_LINTrigError")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    Int32 B = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    Int32 ErrorMode = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    UInt32 Re = OSC_LINTrigError(B, ErrorMode);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_LINConfig
                            if (CAPL_FunctionName == "OSC_LINConfig")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                Pos5 = CAPL_String.IndexOf("@", Pos4 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0)
                                {
                                    Int32 B = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    Int32 BitRate = Convert.ToInt32(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    double Level = Convert.ToDouble(CAPL_String.Substring(Pos4 + 1, Pos5 - Pos4 - 1));
                                    UInt32 Re = OSC_LINConfig(B, Channel, BitRate, Level);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_LINBitRateConfig
                            if (CAPL_FunctionName == "OSC_LINBitRateConfig")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    Int32 B = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    Int32 Baudrate = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    UInt32 Re = OSC_LINBitRateConfig(B, Baudrate);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_TriggerLevel
                            if (CAPL_FunctionName == "OSC_TriggerLevel")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    Int32 source = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    double Level = Convert.ToDouble(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    UInt32 Re = OSC_TriggerLevel(source, Level);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_AcquireConfig
                            if (CAPL_FunctionName == "OSC_AcquireConfig")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    Int32 RecordLength = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    UInt32 Re = OSC_AcquireConfig(RecordLength);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_MeasurVoltage
                            if (CAPL_FunctionName == "OSC_MeasurVoltage")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0)
                                {
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    double Xa = Convert.ToDouble(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    double Xb = Convert.ToDouble(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    double Re = OSC_MeasurVoltage(Channel, Xa, Xb);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_LINTrigMode
                            if (CAPL_FunctionName == "OSC_LINTrigMode")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                if (Pos2 > 0 && Pos3 > 0)
                                {
                                    Int32 B = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    Int32 Mode = Convert.ToInt32(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    UInt32 Re = OSC_LINTrigMode(B, Mode);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_ChannelSet
                            if (CAPL_FunctionName == "OSC_ChannelSet")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                Pos3 = CAPL_String.IndexOf("@", Pos2 + 1);
                                Pos4 = CAPL_String.IndexOf("@", Pos3 + 1);
                                if (Pos2 > 0 && Pos3 > 0 && Pos4 > 0)
                                {
                                    Int32 Channel = Convert.ToInt32(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    double Range = Convert.ToDouble(CAPL_String.Substring(Pos2 + 1, Pos3 - Pos2 - 1));
                                    double Offset = Convert.ToDouble(CAPL_String.Substring(Pos3 + 1, Pos4 - Pos3 - 1));
                                    UInt32 Re = OSC_ChannelSet(Channel, Range, Offset);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_ZoomCfg
                            if (CAPL_FunctionName == "OSC_ZoomCfg")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    double Timebase = Convert.ToDouble(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    UInt32 Re = OSC_ZoomCfg(Timebase);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                            #region OSC_ZoomClose
                            if (CAPL_FunctionName == "OSC_ZoomClose")
                            {
                                UInt32 Re = OSC_ZoomClose();
                                EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                StateMachine_TIME = DateTime.Now;
                                EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                            }
                            #endregion
                            #region OSC_ZoomPosition
                            if (CAPL_FunctionName == "OSC_ZoomPosition")
                            {
                                Pos2 = CAPL_String.IndexOf("@", Pos1 + 1);
                                if (Pos2 > 0)
                                {
                                    double Position = Convert.ToDouble(CAPL_String.Substring(Pos1 + 1, Pos2 - Pos1 - 1));
                                    UInt32 Re = OSC_ZoomPosition(Position);
                                    EnVariable_StateMachine_String.Value = Re.ToString() + "@";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                                else
                                {
                                    EnVariable_StateMachine_String.Value = "InputError";
                                    StateMachine_TIME = DateTime.Now;
                                    EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            EnVariable_StateMachine_String.Value = "InputError";
                            StateMachine_TIME = DateTime.Now;
                            EnVariable_StateMachine_TIME.Value = StateMachine_TIME.Minute * 60 + StateMachine_TIME.Second + ((double)StateMachine_TIME.Millisecond) / 1000;
                        }
                        CAPL_TIME = EnVariable_CAPL_TIME.Value;
                    }
                }
                //Console.WriteLine(EnVariable_StateMachine_String.Value);
                //Console.WriteLine("***************************************");
                Thread.Sleep(1);
            }
        }


        private static DateTime trim(string p)
        {
            throw new NotImplementedException();
        }
    }
}