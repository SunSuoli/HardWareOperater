using System;
using System.Collections;
using System.Data;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using static HardWareOperater.BinDing;

namespace HardWareOperater
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /*1.数字量采集卡常量创建*/
        DI_DAQ di = new DI_DAQ();
        string[] DI_Ports = {
            "离散输入/port0",
            "离散输入/port1",
            "离散输入/port2",
            "离散输入/port3",
            "离散输入/port4",
            "离散输入/port5",
            "离散输入/port6",
            "离散输入/port7"};//通道数组常量
        /*2.Pikering矩阵开关量创建*/
        Matrix_Switch ms = new Matrix_Switch();
        /*3.安捷伦数字万用表*/
        DMM_A34401 dmm = new DMM_A34401();

        Queue q = new Queue();//操作队列

        Source source_DataGrid = new Source();//DataGrid的数据源
        Source source_bool = new Source();//DataGrid的数据源
        
        public MainWindow()
        {
            InitializeComponent();

            /*控件操作*/
            Bind(source_DataGrid, mygrid, DataGrid.ItemsSourceProperty);
            Bind(source_bool, _bool, RadioButton.IsCheckedProperty);

            /*硬件操作*/
            /*1.数字量采集卡初始化*/
            di.Open_Card(DI_Ports);

            /*2.Pikering矩阵开关初始化*/
            ms.Open_Card();
            ms.Select_subunit(0, 0);

            /*3.数字万用表*/
            dmm.Open_Card("COM1", true);
            dmm.Configure(DMM_FUNCT.RES, 0.05, DMM_Resolution._5_5);
            Console.WriteLine(dmm.Value_Read()+10);

            /*启动状态机线程*/
            Thread th_main = new Thread(new ThreadStart(Main));
            th_main.Start();

        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {

            q.Enqueue("start");
            /*设置*/
            ms.CrossPoint_Write(Convert.ToInt32(row.Text), Convert.ToInt32(colunm.Text), (bool)Value.IsChecked);
        }//开始按钮
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            q.Enqueue("stop");
            source_bool.Data_object= ms.CrossPoint_View(Convert.ToInt32(row.Text), Convert.ToInt32(colunm.Text));
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
                            data = di.Digital_Read();
                            /*把数组存到DataTable中*/
                            if (dt.Rows.Count == 0)//如果第一次创建，则初始化DataTable，如果每次初始化的话会闪烁
                            {
                                for (int i = 0; i < data.GetLength(0); i++)//根据数组大小初始化DataTable的列数
                                {
                                    dt.Columns.Add("第" + i.ToString() + "列",typeof(bool));
                                }
                                for (int i = 0; i < data.GetLength(0); i++)
                                {
                                    DataRow row = dt.NewRow();//创建一个行
                                    dt.Rows.Add(row);//把行添加到DataTable
                                }
                            }
                            else
                            {
                                for (int i = 0; i < data.GetLength(0); i++)
                                {
                                    for (int j = 0; j < data.GetLength(1); j++)//填充行内容
                                    {
                                       dt.Rows[i][j] = data[i, j];
                                    }
                                }
                            }
                           
                            source_DataGrid.Data_object = dt.DefaultView;//赋值到DataGrid的数据源
                            break;
                        case "stop":

                            break;
                        case "quiet":
                            di.Close_Card();
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
