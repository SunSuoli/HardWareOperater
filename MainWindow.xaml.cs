using NationalInstruments.DAQmx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static HardWareOperater.BinDing;

namespace HardWareOperater
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Task DI_Task = new Task();//一个数字输入任务
        DigitalMultiChannelReader DigitalReader = null;//定义一个多通道数据流
        string[] DI_Lines = {
            "离散输入/port0", 
            "离散输入/port1",
            "离散输入/port2", 
            "离散输入/port3", 
            "离散输入/port4", 
            "离散输入/port5", 
            "离散输入/port6", 
            "离散输入/port7"};//通道数组常量

        Queue q = new Queue();//操作队列

        Source source = new Source();//绑定数据源

        Source source_1 = new Source();//绑定第一列

        

        public MainWindow()
        {
            InitializeComponent();
            
            /*控件操作*/
            Bind(source, bool_show, TextBlock.TextProperty);//绑定RadioButton

            Bind(source_1, mygrid, DataGrid.ItemsSourceProperty);
            
            
            
            /*硬件操作*/
            foreach (string DI_Line in DI_Lines)//创建多个通道
            {
                DI_Task.DIChannels.CreateChannel(DI_Line,null, ChannelLineGrouping.OneChannelForAllLines);
            }
            DI_Task.Start(); //开始任务/
            DigitalReader = new DigitalMultiChannelReader(DI_Task.Stream);//实例化数据流（LabVIEW中将这一步和数据读取集成到一起了）

            /*启动状态机线程*/
            Thread th_main = new Thread(new ThreadStart(Main));
            th_main.Start();

        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            

            q.Enqueue("start");
        }//开始按钮
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            q.Enqueue("stop");
        }//停止按钮
        private void Quiet(object sender, System.ComponentModel.CancelEventArgs e)
        {
            q.Enqueue("quiet");
        }//关闭窗体
        private void Main()
        {
            /*状态机的两个关键变量*/
            string state="";
            bool run = true;
            /*需要传输的数据*/
            bool[,]data;
            string a = "";
            DataTable dt = new DataTable();
            /*主循环*/
            while (run)
            {
                if (q.Count > 0)//通过队列内容改变状态机的状态
                {
                    state = (string)q.Dequeue();
                }
                else//状态机主体
                {
                    switch (state)
                    {
                        case "start":
                           data= DigitalReader.ReadSingleSampleMultiLine();//多通道（通过实例化数据流已经完成设置），单采集，多线

                            dt=new DataTable();
                            for (int i = 0; i < data.GetLength(0); i++)//初始化一个DataTable
                            {
                                dt.Columns.Add("第"+i.ToString()+"列");
                            }
                            
                            for (int i = 0; i < data.GetLength(0); i++)
                            {
                                DataRow row = dt.NewRow();//创建一个行
                                for (int j = 0; j < data.GetLength(1); j++)
                                {
                                    row[j] = data[i,j].ToString();
                                }
                                dt.Rows.Add(row);//向DataTable中添加一行
                            }
                            source_1.Data_object = dt.DefaultView;
                            
                            break;
                        case "stop":

                            break;
                        case "quiet":
                            DI_Task.Stop();
                            run = false;
                            break;
                        default:
                            break;
                    }
                }
                Thread.Sleep(100);//休眠100ms
            }
        }
    }
}
