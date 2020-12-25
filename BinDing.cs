using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace HardWareOperater
{
    class BinDing
    {
        public class Source : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            //创建Object类型数据源
            private object data_object;
            public object Data_object
            {
                get { return data_object; }
                set
                {
                    data_object = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Data_object"));
                    }
                }
            }
        }
        
        public static void Bind(Source Data_Source, DependencyObject Control_Name, DependencyProperty Property)
        {
            Binding Bind = new Binding();//实例化绑定
            Bind.Source = Data_Source;//设置绑定数据源
            Bind.Path= new PropertyPath("Data_object" );//设置绑定路径
            Bind.Mode = BindingMode.TwoWay;//双向绑定
            Bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;//数据随控件改变
            BindingOperations.SetBinding(Control_Name, Property, Bind);
        }
    }
}
