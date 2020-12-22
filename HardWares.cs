using NationalInstruments.DAQmx;
using Pickering.Lxi.Piplx;
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
    public class DAQ_DI
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
}
