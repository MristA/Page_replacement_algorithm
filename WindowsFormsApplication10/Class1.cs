using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.IO.Ports;
using System.Threading;

namespace WindowsFormsApplication10
{
   public class Comm
　　{
       //委托
        public delegate void EventHandle(byte[] readBuffer); 
       //事件句柄
        public event EventHandle DataReceived; 
       //串口
        public SerialPort serialPort; 
       //线程
　    　Thread thread;
        volatile bool _keepReading; 
   public Comm() 
　　{   //产生实例
　　    serialPort =new SerialPort(); 
       //线程为空
　　    thread =null; 
　　    _keepReading =false; 
　　}
    
   //串口是否打开
   public bool IsOpen 
　　{
        get
　   　 {
              return serialPort.IsOpen; 
　     　}
　　}
   private void StartReading() 
　　{
        if (!_keepReading) 
　　     {
　　          _keepReading =true;
            //thread是线程，用ReadPort方法初始化了ThreadStart代理的(delegate)
　　          thread =new Thread(new ThreadStart(ReadPort)); 
　          　thread.Start();
　　      }
　　}
    private void StopReading() 
　　 {
        if (_keepReading) 
　　      {
　　          _keepReading =false; 
            //抛出InterruptedException异常
　           　thread.Join();
　　           thread =null;                
　　      }
　　}

    //读取串口
     private void ReadPort() 
　　  {
        while (_keepReading) 
　       　{
                if (serialPort.IsOpen)
                {
                    //  获取接收缓冲区中数据的字节数。
                    int count = serialPort.BytesToRead; 
                    if (count > 0) 
　　                  {
                        byte[] readBuffer = new byte[count]; 
                        try  
　　                      {
                        　　Application.DoEvents();
　　                        serialPort.Read(readBuffer,0, count); 
                            if(DataReceived != null) 
　                          DataReceived(readBuffer);
　　                        Thread.Sleep(100); 
　　                       }
                        catch (TimeoutException) 
　　                      {
　　                       }
　　                    }
　　              }
　　          }
　　  }
    public void Open() 
　　{
　　   Close();
　　   serialPort.Open();
       if (serialPort.IsOpen) 
　　      {
　　          StartReading();
　　      }
       else  
　　      {
　         　MessageBox.Show("串口打开失败！"); 
　       　}
　　}
    public void Close() 
　   　{
　       　StopReading();
　       　serialPort.Close();
　   　}
    public  void WritePort(byte[] send, int offSet, int count) 
　   　{
            if (IsOpen) 
　            　{
　                　serialPort.Write(send, offSet, count);
　　             }
　　     }
　　}

}
