using NationalInstruments.DAQmx;
using Pickering.Lxi.Piplx;
using Ivi.Visa;
using NationalInstruments.Visa;
using System.Text;
using System.Threading;

namespace HardWareOperater
{
    
    /*Pickering 的矩阵开关*/
    public class Matrix_Switch
    {
        private PiplxManager manager = new PiplxManager();
        private PiplxCard card = null;
        private MatrixSubunit subunit = null;
        public void Open_Card()//选择卡
        {
            manager.Connect();
        }
        public void Select_subunit(int card_number, int subunit_number)//选择目标单元
        {
            card = (PiplxCard)manager.Cards[card_number];
            card.Open();
            subunit = (MatrixSubunit)card.OutputSubunits[subunit_number];
        }
        public void CrossPoint_Write(int row,int column,bool action)//设置交点值
        {
            subunit.OperateCrosspoint(row, column, action);
        }
        public bool CrossPoint_View(int row, int column)//读取交点值
        {
           return subunit.ViewCrosspoint(row, column);
        }
        public void Close_Card()
        {
            manager.Disconnect();
        }
    }
    /*NI的数字量输入卡*/
    public class DI_DAQ
    {
        private Task DI_Task = new Task();//一个数字输入任务
        private DigitalMultiChannelReader DigitalReader = null;//定义一个多通道数据流
       
        public void Open_Card(string[] DI_Ports)
        {
            foreach (string DI_Port in DI_Ports)//创建多个通道
            {
                DI_Task.DIChannels.CreateChannel(DI_Port, null, ChannelLineGrouping.OneChannelForAllLines);
            }
            DI_Task.Start(); //开始任务/
            DigitalReader = new DigitalMultiChannelReader(DI_Task.Stream);//实例化数据流（LabVIEW中将这一步和数据读取集成到一起了）
        }
        public bool[,] Digital_Read()
        {
            return DigitalReader.ReadSingleSampleMultiLine();//多通道（通过实例化数据流已经完成设置），单采集，多线
        }
        public void Close_Card()
        {
            DI_Task.Stop();
        }
    }
    public class Instrument_IVI
    {
        private ResourceManager rm = new ResourceManager();//实例化一个资源
        protected MessageBasedSession mbSession;
        protected int a;
        public void Open_Card(string resourcename,bool reset)
        {
            mbSession = (MessageBasedSession)rm.Open(resourcename);//打开资源

            mbSession.TimeoutMilliseconds = 10000;
            ParseResult Instr = rm.Parse(resourcename);//为了和LabVIEW里的属性名称保持一致，这里命名为Instr

            if (Instr.InterfaceType== HardwareInterfaceType.Serial)//判断是不是串口设备
            {
                SerialSession serial = (SerialSession)mbSession;

                /*配置串口参数*/
                serial.TerminationCharacterEnabled = true;//终止符
                serial.TerminationCharacter = 0x0A;
                serial.WriteTermination = SerialTerminationMethod.TerminationCharacter;//写最后加终止符对应LabVIEW里“End Mod For Writes”
                serial.ReadTermination= SerialTerminationMethod.TerminationCharacter;//读取最后加终止符对应LabVIEW里“End Mod For Reads”


                serial.BaudRate = 115200;//波特率
                serial.Parity = SerialParity.None;//校验位
                serial.DataBits = 8;//数据位
                serial.StopBits = SerialStopBitsMode.One;//停止位

                serial.FlowControl = SerialFlowControlModes.None;//流控制

                //serial.Flush((IOBuffers)0xC0, true);//VISA清空I/O空缓存区
                //serial.SetBufferSize((IOBuffers)0x30, 4096);//VISA设置I/O空缓存区大小
            }
            else
            {
                mbSession.Clear();//清空资源缓存
            }
            if (reset)
            {
                mbSession.RawIO.Write("*RST;");//复位设备
            }
            Thread.Sleep(100);
            mbSession.RawIO.Write("*ESE 60;*SRE 56;*CLS;:STAT:QUES:ENAB 32767");//默认设置
        }
        public void Close_Card()
        {
            mbSession.Dispose();
        }
    }
    class DMM_IVI: Instrument_IVI
    {
        public enum DMM_FUNCT{VOLT_DC,VOLT_AC,RES,FRES,CURR_DC,CURR_AC,FREQ,PER,CONT,DIOD,VOLT_DC_RAT,TEMP,CAP};//档位枚举常量

        private string[] Functions ={"VOLT:DC",
                        "VOLT:AC",
                        "RES",
                        "FRES",
                        "CURR:DC",
                        "CURR:AC",
                        "FREQ",
                        "PER",
                        "CONT",
                        "DIOD",
                        "VOLT:DC:RAT",
                        "TEMP",
                        "CAP"};
        private string fun_str = ":CONF:";//挡位
        public void Configure(DMM_FUNCT function)
        {
            fun_str += Functions[(int)function];
            if ((int)function < 6 | (int)function == 12)
            {
                fun_str += string.Format("{0:%.;%g,}", 12.0000);
                switch ((int)function)
                {
                    case 2:
                        break;
                    case 3:
                        break;
                    case 4:
                        break;
                    case 5:
                        break;
                    default:
                        break;
                }
            }
            else
            {
                fun_str += ";";
            }
        }
    }
}
