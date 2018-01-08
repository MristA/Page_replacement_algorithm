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
using System.Configuration;
using System.Xml;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.IO;
namespace WindowsFormsApplication10
{

    public partial class 页面置换 : DevExpress.XtraEditors.XtraForm
    {
        public 页面置换()
        {
           
            InitializeComponent();
            //Control.CheckForIllegalCrossThreadCalls = false;
            this.gridView1.IndicatorWidth = 30;
            this.gridView2.IndicatorWidth = 30;
            this.gridView3.IndicatorWidth = 30;
            this.gridView4.IndicatorWidth = 30;
        }
        private void gridView_CustomDrawRowIndicator(object sender, DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
        {

            if (e.Info.IsRowIndicator && e.RowHandle >= 0)
            {

                e.Info.DisplayText = (e.RowHandle + 1).ToString();

            }

        }

        void gridView_RowStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowStyleEventArgs e)
        {
            DevExpress.XtraGrid.Views.Grid.GridView view = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            if (e.RowHandle >=0)
            {
                string category = view.GetRowCellDisplayText(e.RowHandle, view.Columns[2]);
                if (category == "是")
                {
                    e.Appearance.BackColor = System.Drawing.Color.OrangeRed;
                    //e.Appearance.BackColor2 = Color.SeaShell;
                }
                else e.Appearance.BackColor = System.Drawing.Color.CornflowerBlue;
            }
        }
        
        
        
        SerialPort sp1 = new SerialPort();
        //Control.CheckForIllegalCrossThreadCalls = false;    //意图见解释  
        //sp1.DataReceived += new SerialDataReceivedEventHandler(sp1_DataReceived); //订阅委托  

        int num_page_frame;//是内存块数;
        int num_page;//为内存页数，
        int num_tlb;
        bool tlb_check;
        int time_memory;//为内存存取时间，
        int time_break;//为中断时间，
        int time_tlb;//为快表存取时间，
        string p_squ;//
        string fou="否";
        string shi = "是";
        string wu = "无";
        int p_count=0;
        int count =0;
        int run_t = 0;
        int t_slepp = 1000;
        static Semaphore sem = new Semaphore(1, 1);
        Thread thread_FIFO = null;
        Thread thread_LRU = null;
        Thread thread_LFU = null;
        Thread thread_OPT= null;
        Thread run_thread = null;
        int t_check;
        int m_check ;
        int n_check;
        int FIFO_ALL_TIME;
        int FIFO_AVERAGE_TIME;
        int LFU_ALL_TIME;
        int LFU_AVERAGE_TIME;
        int LRU_ALL_TIME;
        int LRU_AVERAGE_TIME;
        int OPT_ALL_TIME;
        int OPT_AVERAGE_TIME;

        DataTable TIME_AL = new DataTable();
        private delegate void SafeSetGridControl(DataTable v);
        private delegate void SafeSetLabelControl(string v);
        private delegate void SafeSetbuttonControl(string v);
        private delegate void SafeSetGridControl_refresh( );
        private delegate void SafeSetchartControl_refresh(DataTable v);
        private delegate void SafeSetchartControl(DataTable v);
        private void OnSafeSetChartControl_bin(DevExpress.XtraCharts.ChartControl chartControl, DataTable va)
        {

            if (chartControl.InvokeRequired)
            {

                SafeSetchartControl_refresh call = delegate(DataTable v)
                {

                    DevExpress.XtraCharts.Series s1 = chartControl.Series[0];
                    s1.DataSource = v;
                    s1.ValueDataMembers[0] = "Value";
                    s1.ArgumentDataMember = "Name";
                 //   s1.LegendPointOptions.PointView =  DevExpress.XtraCharts.PointView.ArgumentAndValues;
                   // s1.ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Qualitative;
                    s1.ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Qualitative;//定性的  
                    s1.ValueScaleType = DevExpress.XtraCharts.ScaleType.Numerical;//数字类型  
                    s1.PointOptions.PointView = DevExpress.XtraCharts.PointView.ArgumentAndValues;//显示表示的信息和数据  
                    s1.PointOptions.ValueNumericOptions.Format = DevExpress.XtraCharts.NumericFormat.Percent;//用百分比表示  
                    //// 以哪个字段进行显示 
                    //s1.ArgumentDataMember = "type";
                    //s1.ValueScaleType = DevExpress.XtraCharts.ScaleType.Numerical;

                    //// 柱状图里的柱的取值字段 
                    //s1.ValueDataMembers.AddRange(new string[] { "ALL_T" });
                    //DevExpress.XtraCharts.Series s2 = chartControl.Series[1];
                    //s2.DataSource = v;
                    //s2.ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Qualitative;

                    //// 以哪个字段进行显示 
                    //s2.ArgumentDataMember = "type";
                    //s2.ValueScaleType = DevExpress.XtraCharts.ScaleType.Numerical;

                    //// 柱状图里的柱的取值字段 
                    //s2.ValueDataMembers.AddRange(new string[] { "AVGERAGE_T" });
                };
                chartControl.Invoke(call, va);
            }
            else
            {


                DevExpress.XtraCharts.Series s1 = chartControl.Series[0];
                s1.DataSource = va;
                s1.ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Qualitative;

                // 以哪个字段进行显示 
                s1.ArgumentDataMember = "type";
                s1.ValueScaleType = DevExpress.XtraCharts.ScaleType.Numerical;

                // 柱状图里的柱的取值字段 
                s1.ValueDataMembers.AddRange(new string[] { "ALL_T" });
                DevExpress.XtraCharts.Series s2 = chartControl.Series[1];
                s2.DataSource = va;
                s2.ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Qualitative;

                // 以哪个字段进行显示 
                s2.ArgumentDataMember = "type";
                s2.ValueScaleType = DevExpress.XtraCharts.ScaleType.Numerical;

                // 柱状图里的柱的取值字段 
                s2.ValueDataMembers.AddRange(new string[] { "AVGERAGE_T" });
            }
        }
         private void OnSafeSetChartControl(DevExpress.XtraCharts.ChartControl chartControl, DataTable va)
         {

             if (chartControl.InvokeRequired)
             {

                 SafeSetchartControl_refresh call = delegate(DataTable v)
                 {

                     DevExpress.XtraCharts.Series s1 = chartControl.Series[0];
                     s1.DataSource = v;
                     s1.ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Qualitative;

                     // 以哪个字段进行显示 
                     s1.ArgumentDataMember = "type";
                     s1.ValueScaleType = DevExpress.XtraCharts.ScaleType.Numerical;

                     // 柱状图里的柱的取值字段 
                     s1.ValueDataMembers.AddRange(new string[] { "ALL_T" });
                     DevExpress.XtraCharts.Series s2 = chartControl.Series[1];
                     s2.DataSource = v;
                     s2.ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Qualitative;

                     // 以哪个字段进行显示 
                     s2.ArgumentDataMember = "type";
                     s2.ValueScaleType = DevExpress.XtraCharts.ScaleType.Numerical;

                     // 柱状图里的柱的取值字段 
                     s2.ValueDataMembers.AddRange(new string[] { "AVGERAGE_T" });
                 };
                 chartControl.Invoke(call, va);
             }
             else
             {


                 DevExpress.XtraCharts.Series s1 = chartControl.Series[0];
                 s1.DataSource = va;
                 s1.ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Qualitative;

                 // 以哪个字段进行显示 
                 s1.ArgumentDataMember = "type";
                 s1.ValueScaleType = DevExpress.XtraCharts.ScaleType.Numerical;

                 // 柱状图里的柱的取值字段 
                 s1.ValueDataMembers.AddRange(new string[] { "ALL_T" });
                 DevExpress.XtraCharts.Series s2 = chartControl.Series[1];
                 s2.DataSource = va;
                 s2.ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Qualitative;

                 // 以哪个字段进行显示 
                 s2.ArgumentDataMember = "type";
                 s2.ValueScaleType = DevExpress.XtraCharts.ScaleType.Numerical;

                 // 柱状图里的柱的取值字段 
                 s2.ValueDataMembers.AddRange(new string[] { "AVGERAGE_T" });
             }
         }
         private void OnSafeSetGridControl(DevExpress.XtraGrid.GridControl gridcontrol , DataTable va)
         {

             if (gridcontrol.InvokeRequired)
             {

                 SafeSetGridControl call = delegate(DataTable v) { gridcontrol.DataSource = v;  };
                 gridcontrol.Invoke(call, va);
             }
             else
             {
                
                 gridcontrol.DataSource = va;
             }
         }
         private void OnSafeSetGridControl_Refresh(DevExpress.XtraGrid.GridControl gridcontrol)
         {

             if (gridcontrol.InvokeRequired)
             {

                 SafeSetGridControl_refresh call = delegate() {  gridcontrol.Refresh(); };
                 gridcontrol.Invoke(call);
             }
             else
             {
                 gridcontrol.Refresh();
             }
         }

         private void OnSafeSetLabelControl(DevExpress.XtraEditors.LabelControl lable, string va)
         {

             if (lable.InvokeRequired)
             {

                 SafeSetLabelControl call = delegate(string v) { lable.Text = v; };
                 lable.Invoke(call, va);
             }
             else
             {
                 lable.Text = va; 
             }
         }
         private void OnSafeSetButtonControl(DevExpress.XtraEditors.SimpleButton button, string va)
         {

             if (button.InvokeRequired)
             {

                 SafeSetLabelControl call = delegate(string v) { button.Text = v; };
                 button.Invoke(call, va);
             }
             else
             {
                 button.Text = va;
             }
         }
       
        private void 串口_Load(object sender, EventArgs e)
        {
            check_tlb.EditValue = true;
            tlb_check = true;
            read("record.txt");
            TIME_AL.Columns.Add("type", typeof(string));
            TIME_AL.Columns.Add("ALL_T",typeof(int));
            TIME_AL.Columns.Add("AVGERAGE_T",typeof(int));
           
        }

        void read(string filename)
        {
            if (File.Exists(filename))
            { 
                StreamReader objReader = new StreamReader(filename);
              

                page_frame_num.Text = objReader.ReadLine();
                page_num.Text = objReader.ReadLine();
                page_time.Text = objReader.ReadLine();
                missing_page_time.Text = objReader.ReadLine();
                page_sequence.Text = objReader.ReadLine();
                if (objReader.ReadLine() == "True")
                {

                    tlb_num.Text = objReader.ReadLine();
                    tlb_time.Text = objReader.ReadLine();
                }
                else
                {
                    check_tlb.EditValue = false;
                    tlb_check = false;
                }
              //  tlb_time.Text = objReader.ReadLine();
                objReader.Close();

            }
        }





        private void timer1_Tick(object sender, EventArgs e)
        {
            if(thread_FIFO==null&&thread_LFU==null&&thread_LRU==null&&thread_OPT==null)
            {run_t = 0;
             OnSafeSetButtonControl(run, "开始运行");
            }
            
        }

        private void rand_CheckedChanged_1(object sender, EventArgs e)
        {
            if (rand.Checked)
            {
                Random ran = new Random();
                page_frame_num.Text = ran.Next(2, 6).ToString();
                page_num.Text = ran.Next(5, 10).ToString();
                page_time.Text = ran.Next(10, 30).ToString();
                missing_page_time.Text = ran.Next(100, 300).ToString();
                int n = ran.Next(15, 20);
                string now_p_squ = "";
                Boolean s1 = ran.Next(0, 2)==0?false:true;
                check_tlb.EditValue = s1;
                if (s1)
                {
                    tlb_time.Text = ran.Next(2, 5).ToString();
                    tlb_num.Text = ran.Next(1, int.Parse(page_frame_num.Text) + 1).ToString();
                }
                for (int i = 0; i < n-1; i++)
                {
                    now_p_squ += ran.Next(0, int.Parse(page_num.Text)).ToString();
                    now_p_squ += ",";

                }
                page_sequence.Text = now_p_squ;
            }
            Boolean s = rand.Checked;
            layoutControlGroup4.Enabled = !s;
            layoutControlGroup3.Enabled = !s;
            page_sequence.Enabled = !s;
            reset.Enabled = !s;

        }


        private void reset_Click(object sender, EventArgs e)
        {
            page_frame_num.Text = page_num.Text = page_sequence.Text = page_time.Text = missing_page_time.Text =  "";
            if ((Boolean)check_tlb.EditValue == true)
            {
                tlb_time.Text = "";
                tlb_num.Text = "";
            }
        }
        void FIFO()
        {
            DataTable db_FIFO = new DataTable();
            db_FIFO.Columns.Add("FIFO_MEMORY");
            db_FIFO.Columns.Add("FIFO_PAGE_NEEDED");
            db_FIFO.Columns.Add("FIFO_MISSED_PAGE");
            db_FIFO.Columns.Add("FIFO_OUT_PAGE");
            db_FIFO.Columns.Add("FIFO_NUM_OUT_PAGE");
            db_FIFO.Columns.Add("FIFO_TIME");
            int tlb_p = 0;
            string n_s = "";
            string[] str2 = System.Text.RegularExpressions.Regex.Split(p_squ, @",");
            string[] str3 = new string[num_page_frame];
            string[] tlb=new string[num_tlb];
            int m_page = 0;
            int m_tlb = 0;
            int all_time = 0;
            int now_time = 0;
          
            int num_break = 0;
            for (int i = 0; i < p_count; i++)
            {
               // OnSafeSetGridControl(gridControl1, null);
                DataRow dr = db_FIFO.NewRow();
                dr[0] = n_s;
                string need_page = str2[i];
                dr[1] = need_page;
                int m_t = 0;
                for (int j = 0; j < m_tlb; j++)
                {
                    if (need_page == tlb[j])
                    {
                        m_t = 1;
                        now_time = t_check;
                        tlb_p++;
                        break;
                    }
                }
                if (m_t == 0)
                for (int j = 0; j < m_page; j++)
                {
                    if (need_page == str3[j])
                    {
                        m_t = 2;
                        now_time = m_check;
                        if (m_tlb < num_tlb)
                        {
                            tlb[m_tlb] = need_page;
                            m_tlb++;
                        }
                        else
                        {
                            for (int u = 0; u < num_tlb - 1; u++)
                            {
                                tlb[u] = tlb[u + 1];
                            }
                             if(num_tlb!=0)  tlb[num_tlb - 1] = need_page;
                        }
                        break;
                    }

                }
                if (m_t!=0)
                {
                    dr[2] = fou;
                    dr[3] = fou;
                    dr[4] = wu;
                    dr[5] = now_time.ToString();
                    all_time += now_time;
                    db_FIFO.Rows.Add(dr);
                }
                else
                {
                    num_break++;
                    dr[2] = shi;
                    if (m_page < num_page_frame)
                    {
                        dr[3] = fou;
                        dr[4] = wu;
                        str3[m_page] = need_page;
                        m_page++;

                    }
                    else
                    {
                        dr[3] = shi;
                        dr[4] = str3[0];
                        for (int y = 0; y < m_page - 1; y++) str3[y] = str3[y + 1];
                        str3[m_page - 1] = need_page;
                    }
                    if (m_tlb < num_tlb)
                    {
                        tlb[m_tlb] = need_page;
                        m_tlb++;
                    }
                    else
                    {
                        int u=0;
                        for ( u = 0; u < num_tlb; u++)
                        {
                            if (dr[4] == tlb[u]) break;
                        }
                        u = u == num_tlb ? 0 : u;
                        for (; u < num_tlb - 1; u++)
                        {
                            tlb[u] = tlb[u + 1];
                        }
                         if(num_tlb!=0)  tlb[u] = need_page;
                    }
                    n_s = "";
                    now_time =n_check ;
                    //MessageBox.Show(n_check.ToString());
                    dr[5] = now_time.ToString();
                    all_time += now_time;
                    for (int y = 0; y < m_page - 1; y++)
                    {
                        n_s += str3[y] + ",";
                    }
                    n_s += str3[m_page - 1];
                    db_FIFO.Rows.Add(dr);

                }
                DataTable dt2 = new DataTable();
                dt2 = db_FIFO.Copy();
                OnSafeSetGridControl(gridControl1, dt2);
                OnSafeSetGridControl_Refresh(gridControl1);
                Thread.Sleep(now_time);
            }
            FIFO_ALL_TIME = all_time;
            FIFO_AVERAGE_TIME=(all_time / (p_count));
            OnSafeSetLabelControl(FIFO_BREAK_NUM, num_break.ToString());
            DataTable table = new DataTable("Table1");
            table.Columns.Add("Name", typeof(String));
            table.Columns.Add("Value", typeof(Int32));
            table.Rows.Add(new object[] { "快表命中", tlb_p });
            table.Rows.Add(new object[] { "中断", num_break });
            table.Rows.Add(new object[] { "页表命中", p_count - num_break - tlb_p });
            OnSafeSetChartControl_bin(chartControl_fifo, table);
        }

        void LFU()
        {
            DataTable db_LFU = new DataTable();
            
            db_LFU.Columns.Add("LFU_MEMORY");
            db_LFU.Columns.Add("LFU_PAGE_NEEDED");
            db_LFU.Columns.Add("LFU_MISSED_PAGE");
            db_LFU.Columns.Add("LFU_OUT_PAGE");
            db_LFU.Columns.Add("LFU_NUM_OUT_PAGE");
            db_LFU.Columns.Add("LFU_TIME");
            string n_s = "";
            int tlb_p = 0;
            string[] str2 = System.Text.RegularExpressions.Regex.Split(p_squ, @",");
            string[] str3 = new string[num_page_frame];
            int[] str_count = new int[num_page_frame];
            int m_page = 0;
            int all_time = 0;
            int m_tlb = 0;
            int average_time = 0;
            int num_break = 0;

            string[] tlb = new string[num_tlb];
            int now_time = 0;

            for (int i = 0; i < p_count; i++)
            {
               // OnSafeSetGridControl(gridControl2, null);
                DataRow dr = db_LFU.NewRow();
                dr[0] = n_s;
                string need_page = str2[i];
                dr[1] = need_page;
                int m_t = 0;
                for (int j = 0; j < m_tlb; j++)
                {
                    if (need_page == tlb[j])
                    {
                        m_t = 1;
                        str_count[j]++;
                        now_time = t_check;
                        tlb_p++;
                        break;
                    }
                }
                for (int j = 0; j < m_page; j++)
                {
                    if (need_page == str3[j])
                    {
                        str_count[j]++;
                        if(m_t!=1)
                        now_time = m_check;
                        m_t = 2;
                        if (m_tlb < num_tlb)
                        {
                            tlb[m_tlb] = need_page;
                            m_tlb++;
                        }
                        else
                        {
                            for (int u = 0; u < num_tlb - 1; u++)
                            {
                                tlb[u] = tlb[u + 1];
                            }
                             if(num_tlb!=0)  tlb[num_tlb - 1] = need_page;
                        }
                        break;
                    }
                }
                if (m_t!=0)
                {
                    dr[2] = fou;
                    dr[3] = fou;
                    dr[4] = wu;
                    dr[5] = now_time.ToString();
                    all_time += now_time;
                    db_LFU.Rows.Add(dr);
                }
                else
                {
                    num_break++;
                    dr[2] = shi;
                    if (m_page < num_page_frame)
                    {
                        str_count[m_page] = 1;
                        dr[3] = fou;
                        dr[4] = wu;
                        str3[m_page] = need_page;
                        m_page++;

                    }
                    else
                    {
                        dr[3] = shi;
                        int str_min = 0;
                        int min = str_count[0];
                        for (int y = 1; y < m_page; y++)
                            if (min > str_count[y])
                            {
                                min = str_count[y];
                                str_min = y;
                            }
                        dr[4] = str3[str_min];
                        for (int y = str_min; y < m_page - 1; y++)
                        {
                            str3[y] = str3[y + 1];
                            str_count[y] = str_count[y + 1];
                        }
                        str3[m_page - 1] = need_page;
                        str_count[m_page - 1] = 1;
                    }
                    if (m_tlb < num_tlb)
                    {
                        tlb[m_tlb] = need_page;
                        m_tlb++;
                    }
                    else
                    {
                        int u = 0;
                        for (u = 0; u < num_tlb; u++)
                        {
                            if (dr[4] == tlb[u]) break;
                        }
                        u = u == num_tlb ? 0 : u;
                        for (; u < num_tlb - 1; u++)
                        {
                            tlb[u] = tlb[u + 1];
                        }
                         if(num_tlb!=0)  tlb[u] = need_page;
                    }
                    n_s = "";
                    now_time = n_check;
                    dr[5] = now_time.ToString();
                    all_time += now_time;
                    for (int y = 0; y < m_page - 1; y++)
                    {
                        n_s += str3[y] + ",";
                    }
                    n_s += str3[m_page - 1];
                    db_LFU.Rows.Add(dr);
                }
                DataTable dt2 = new DataTable();
                dt2 = db_LFU.Copy();
                OnSafeSetGridControl(gridControl2, dt2);
                OnSafeSetGridControl_Refresh(gridControl2);
               Thread.Sleep(now_time);
            }
            //this.gridControl2.DataSource = db_LFU;
            //this.Refresh();
            //LFU_ALL_TIME.Text = all_time.ToString();
            //LFU_AVERAGE_TIME.Text = (all_time / (p_count)).ToString();
            //LFU_BREAK_NUM.Text = num_break.ToString();

            LFU_ALL_TIME = all_time;
            LFU_AVERAGE_TIME = (all_time / (p_count));
            OnSafeSetLabelControl(LFU_BREAK_NUM, num_break.ToString());
            DataTable table = new DataTable("Table1");
            table.Columns.Add("Name", typeof(String));
            table.Columns.Add("Value", typeof(Int32));
            table.Rows.Add(new object[] { "快表命中", tlb_p });
            table.Rows.Add(new object[] { "中断", num_break });
            table.Rows.Add(new object[] { "页表命中", p_count - num_break - tlb_p });
            OnSafeSetChartControl_bin(chartControl_lfu, table);
        }

        void LRU()
        {
            DataTable db_LRU = new DataTable();
            db_LRU.Columns.Add("LRU_MEMORY");
            db_LRU.Columns.Add("LRU_PAGE_NEEDED");
            db_LRU.Columns.Add("LRU_MISSED_PAGE");
            db_LRU.Columns.Add("LRU_OUT_PAGE");
            db_LRU.Columns.Add("LRU_NUM_OUT_PAGE");
            db_LRU.Columns.Add("LRU_TIME");
            string n_s = "";
            int tlb_p = 0;
            string[] str2 = System.Text.RegularExpressions.Regex.Split(p_squ, @",");
            string[] str3 = new string[num_page_frame];
            int m_page = 0;
            int all_time = 0;
            int average_time = 0;
            int num_break = 0;
            string[] tlb = new string[num_tlb];
            int now_time = 0;
            int m_tlb = 0;
            for (int i = 0; i < p_count; i++)
            {
               // OnSafeSetGridControl(gridControl3, null);
                DataRow dr = db_LRU.NewRow();
                dr[0] = n_s;
                string need_page = str2[i];
                dr[1] = need_page;
                int m_t = 0;
                for (int j = 0; j < m_tlb; j++)
                {
                    if (need_page == tlb[j])
                    {
                        m_t = 1;
                        now_time = t_check;
                        tlb_p++;
                        break;
                    }
                }
                for (int j = 0; j < m_page; j++)
                {
                    if (need_page == str3[j])
                    {
                      
                        for (; j < m_page - 1; j++)
                            str3[j] = str3[j + 1];
                        str3[j] = need_page;
                        if (m_t == 0)
                           now_time = m_check;
                        m_t = 2;
                        if (m_tlb < num_tlb)
                        {
                            tlb[m_tlb] = need_page;
                            m_tlb++;
                        }
                        else
                        {
                            for (int u = 0; u < num_tlb - 1; u++)
                            {
                                tlb[u] = tlb[u + 1];
                            }
                             if(num_tlb!=0)  tlb[num_tlb - 1] = need_page;
                        }
                        break;

                    }
                }
                if (m_t!=0)
                {
                    dr[2] = fou;
                    dr[3] = fou;
                    dr[4] = wu;
                    dr[5] = now_time.ToString();
                    all_time += now_time;
                    db_LRU.Rows.Add(dr);
                }
                else
                {
                    num_break++;
                    dr[2] = shi;
                    if (m_page < num_page_frame)
                    {
                        dr[3] = fou;
                        dr[4] = wu;
                        str3[m_page] = need_page;
                        m_page++;

                    }
                    else
                    {
                        dr[3] = shi;
                        dr[4] = str3[0];
                        for (int y = 0; y < m_page - 1; y++) str3[y] = str3[y + 1];
                        str3[m_page - 1] = need_page;
                    }
                    if (m_tlb < num_tlb)
                    {
                        tlb[m_tlb] = need_page;
                        m_tlb++;
                    }
                    else
                    {
                        int u = 0;
                        for (u = 0; u < num_tlb; u++)
                        {
                            if (dr[4] == tlb[u]) break;
                        }
                        u = u == num_tlb ? 0 : u;
                        for (; u < num_tlb - 1; u++)
                        {
                            tlb[u] = tlb[u + 1];
                        }
                         if(num_tlb!=0)  tlb[u] = need_page;
                    }
                    n_s = "";
                    now_time = n_check;
                    dr[5] = now_time.ToString();
                    all_time += now_time;
                    for (int y = 0; y < m_page - 1; y++)
                    {
                        n_s += str3[y] + ",";
                    }
                    n_s += str3[m_page - 1];
                    db_LRU.Rows.Add(dr);
                }
                DataTable dt2 = new DataTable();
                dt2 = db_LRU.Copy();
                OnSafeSetGridControl(gridControl3, dt2);
                OnSafeSetGridControl_Refresh(gridControl3);
               Thread.Sleep(now_time);
            }


            //this.gridControl3.DataSource = db_LRU;

            //this.gridControl3.Refresh();
            //LRU_ALL_TIME.Text = all_time.ToString();
            //LRU_AVERAGE_TIME.Text = (all_time / (p_count)).ToString();
            //LRU_BREAK_NUM.Text = num_break.ToString();
          
           LRU_ALL_TIME=all_time;
           LRU_AVERAGE_TIME=all_time / (p_count);
            OnSafeSetLabelControl(LRU_BREAK_NUM, num_break.ToString());
            DataTable table = new DataTable("Table1");
            table.Columns.Add("Name", typeof(String));
            table.Columns.Add("Value", typeof(Int32));
            table.Rows.Add(new object[] { "快表命中", tlb_p });
            table.Rows.Add(new object[] { "中断", num_break });
            table.Rows.Add(new object[] { "页表命中", p_count - num_break - tlb_p });
            OnSafeSetChartControl_bin(chartControl_lru, table);
        }
        void OPT()
        {
            DataTable db_OPT = new DataTable();
            db_OPT.Columns.Add("OPT_MEMORY");
            db_OPT.Columns.Add("OPT_PAGE_NEEDED");
            db_OPT.Columns.Add("OPT_MISSED_PAGE");
            db_OPT.Columns.Add("OPT_OUT_PAGE");
            db_OPT.Columns.Add("OPT_NUM_OUT_PAGE");
            db_OPT.Columns.Add("OPT_TIME");
            string n_s = "";
            int tlb_p = 0;
            string[] str2 = System.Text.RegularExpressions.Regex.Split(p_squ, @",");
            string[] str3 = new string[num_page_frame];
            int[] str_count = new int[num_page_frame];
            int m_page = 0;
            int all_time = 0;
            int average_time = 0;
            int num_break = 0;
            string[] tlb = new string[num_tlb];
            int now_time = 0;
            int m_tlb = 0;
            for (int i = 0; i < p_count; i++)
            {
               // OnSafeSetGridControl(gridControl4, null);
                DataRow dr = db_OPT.NewRow();
                dr[0] = n_s;
                string need_page = str2[i];
                dr[1] = need_page;
                int m_t = 0;
                for (int j = 0; j < m_tlb; j++)
                {
                    if (need_page == tlb[j])
                    {
                        m_t = 1;
                        now_time = t_check;
                        tlb_p++;
                        break;
                    }
                }
                for (int j = 0; j < m_page; j++)
                {
                    if (need_page == str3[j])
                    {
                        str_count[j]++;
                        if(m_t!=1)
                            now_time = m_check;
                        m_t = 2;
                        if (m_tlb < num_tlb)
                        {
                            tlb[m_tlb] = need_page;
                            m_tlb++;
                        }
                        else
                        {
                            for (int u = 0; u < num_tlb - 1; u++)
                            {
                                tlb[u] = tlb[u + 1];
                            }
                             if(num_tlb!=0)  tlb[num_tlb - 1] = need_page;
                        }
                        break;
                    }
                }
                if (m_t!=0)
                {
                    dr[2] = fou;
                    dr[3] = fou;
                    dr[4] = wu;
                    dr[5] = now_time.ToString();
                    all_time += now_time;
                    db_OPT.Rows.Add(dr);
                }
                else
                {
                    num_break++;
                    dr[2] = shi;
                    if (m_page < num_page_frame)
                    {
                        str_count[m_page] = 1;
                        dr[3] = fou;
                        dr[4] = wu;
                        str3[m_page] = need_page;
                        m_page++;

                    }
                    else
                    {
                        dr[3] = shi;
                        int str_min = 0;
                        int max = -1;
                        int mc = 0;
                        for (int h = 0; h < m_page; h++)
                        {
                            str_count[h] = -1;
                        }
                        //MessageBox.Show(str2[5] + str3[0]);
                        //MessageBox.Show((str2[5] == str3[0]).ToString());
                        //MessageBox.Show((str_count[0] == -1).ToString());
                        for (int y = i + 1; y < p_count && mc < m_page; y++)
                        {
                            for (int h = 0; h < m_page; h++)
                            {
                                if (str2[y] == str3[h] && str_count[h] == -1)
                                {

                                    str_count[h] = y;
                                    max = h;
                                    mc++;
                                }
                            }
                        }
                        //MessageBox.Show("max:" + max.ToString() + "  mc:" + mc.ToString() + "  str3[0]:" + str3[0] + "  str_count[0]:" + str_count[0].ToString() + "  str3[1]:" + str3[1] + "  str_count[1]:" + str_count[1].ToString() + "  str3[2]:" + str3[2] + "  str_count[2]:" + str_count[2].ToString());
                        if (mc < m_page)
                            for (int h = 0; h < m_page; h++)
                            {
                                if (str_count[h] == -1)
                                {
                                    dr[4] = str3[h];
                                    str3[h] = need_page;
                                    break;
                                }
                            }
                        else
                        {
                            dr[4] = str3[max];
                            str3[max] = need_page;

                        }
                    }
                    if (m_tlb < num_tlb)
                    {
                        tlb[m_tlb] = need_page;
                        m_tlb++;
                    }
                    else
                    {
                        int u = 0;
                        for (u = 0; u < num_tlb; u++)
                        {
                            if (dr[4] == tlb[u]) break;
                        }
                        u = u == num_tlb ? 0 : u;
                        for (; u < num_tlb - 1; u++)
                        {
                            tlb[u] = tlb[u + 1];
                        }
                         if(num_tlb!=0)  tlb[u] = need_page;
                    }
                    n_s = "";
                    now_time = n_check;
                    dr[5] = now_time.ToString();
                    all_time += now_time;
                    for (int y = 0; y < m_page - 1; y++)
                    {
                        n_s += str3[y] + ",";
                    }
                    n_s += str3[m_page - 1];
                    db_OPT.Rows.Add(dr);
                }
                DataTable dt2 = new DataTable();
                dt2 = db_OPT.Copy();
                OnSafeSetGridControl(gridControl4, dt2);
                OnSafeSetGridControl_Refresh(gridControl4);
               Thread.Sleep(now_time);
            }
           //this.gridControl4.DataSource = db_OPT;
           //this.gridControl4.Refresh();
           // OPT_ALL_TIME.Text = all_time.ToString();
           // OPT_AVERAGE_TIME.Text = (all_time / p_count).ToString();
           // OPT_BREAK_NUM.Text = num_break.ToString();
          
            OPT_ALL_TIME= all_time;
            OPT_AVERAGE_TIME=all_time / (p_count);
            OnSafeSetLabelControl(OPT_BREAK_NUM, num_break.ToString());
            DataTable table = new DataTable("Table1");
            table.Columns.Add("Name", typeof(String));
            table.Columns.Add("Value", typeof(Int32));
            table.Rows.Add(new object[] { "快表命中", tlb_p });
            table.Rows.Add(new object[] { "中断", num_break });
            table.Rows.Add(new object[] { "页表命中", p_count - num_break - tlb_p });
            OnSafeSetChartControl_bin(chartControlopt, table);
        }

        void run_page()
        {
            //FIFO();
            //LFU();
            //LRU();
            //OPT();
             t_check = time_tlb + time_memory;
             m_check = 2 * time_memory;
             n_check = m_check + time_break;
            //MessageBox.Show(n_check.ToString());
              thread_FIFO = new Thread(new ThreadStart(FIFO));
              thread_LRU = new Thread(new ThreadStart(LRU));
              thread_LFU = new Thread(new ThreadStart(LFU));
              thread_OPT = new Thread(new ThreadStart(OPT));
            thread_FIFO.Start();
            thread_LRU.Start();
            thread_LFU.Start();
            thread_OPT.Start();
            thread_FIFO.Join();
            thread_FIFO = null;
            thread_LRU.Join();
            thread_LRU = null;
            thread_LFU.Join();
            thread_LFU = null;
            thread_OPT.Join();
            thread_OPT=null;
            DataRow DR_FIFO = TIME_AL.NewRow();
            DR_FIFO[0] = "FIFO";
            DR_FIFO[1] = FIFO_ALL_TIME;
            DR_FIFO[2] =FIFO_AVERAGE_TIME;
            DataRow DR_LFU = TIME_AL.NewRow();
            DR_LFU[0] = "LFU";
            DR_LFU[1] = LFU_ALL_TIME;
            DR_LFU[2] = LFU_AVERAGE_TIME;
            DataRow DR_LRU = TIME_AL.NewRow();
            DR_LRU[0] = "LRU";
            DR_LRU[1] = LRU_ALL_TIME;
            DR_LRU[2] = LRU_AVERAGE_TIME;
            DataRow DR_OPT = TIME_AL.NewRow();
            DR_OPT[0] = "OPT";
            DR_OPT[1] = OPT_ALL_TIME;
            DR_OPT[2] = OPT_AVERAGE_TIME;
            TIME_AL.Rows.Add(DR_FIFO);
            TIME_AL.Rows.Add(DR_LFU);
            TIME_AL.Rows.Add(DR_LRU);
            TIME_AL.Rows.Add(DR_OPT);
            OnSafeSetChartControl(chartControl1, TIME_AL);
            sem.WaitOne();
            run_t = 0;
             OnSafeSetButtonControl(run, "开始运行");
           // Thread oThread = new Thread(new ThreadStart(FIFO));
           //// MessageBox.Show("tttt");
           // oThread.Start();
           // //while (!oThread.IsAlive)
           // //    Thread.Sleep(1);
           // oThread.Abort();
           // oThread.Join();
        }

        private void run_Click(object sender, EventArgs e)
        {
           // sem.WaitOne();
            if(run_t==0)
            {
                bool t = true;
                if (page_frame_num.Text != null && page_num.Text != null && page_sequence.Text != null && page_time.Text != null && missing_page_time.Text != null && ((Boolean)check_tlb.EditValue == false || ((Boolean)check_tlb.EditValue == true && tlb_num.Text != null && tlb_time.Text != null)))
                {
                    if (int.Parse(page_frame_num.Text) == 0 && int.Parse(page_num.Text) == 0)
                    {
                        t = false;
                    }
                    else
                    {
                        string p_pages = page_num.Text;
                        // MessageBox.Show(p_pages);
                        string[] str2 = System.Text.RegularExpressions.Regex.Split(page_sequence.Text, @",");
                        //MessageBox.Show(str2[0]+p_pages);
                        //MessageBox.Show(str2[0].Length.ToString()+"dd" + p_pages.Length.ToString());
                        //MessageBox.Show(string.CompareOrdinal(p_pages, str2[0]).ToString());
                        p_count = str2.Length - (str2[str2.Length - 1] == "" ? 1 : 0);
                        foreach (string i in str2)
                        {
                            //  MessageBox.Show(i.ToString());
                            if (p_pages.Length <= i.Length)
                            {
                                if (p_pages.Length == i.Length && string.CompareOrdinal(p_pages, i) > 0)
                                {
                                    continue;
                                }
                                t = false;
                                break;
                            }
                        }
                    }
                    
                }
                else t = false;
                if (t == false) MessageBox.Show("数据输入不规范！");
                else
                {
                    // m_page = int.Parse(page_num.Text);
                    num_page = int.Parse(page_num.Text);
                    num_page_frame = int.Parse(page_frame_num.Text);
                    time_memory = int.Parse(page_time.Text);
                    time_break = int.Parse(missing_page_time.Text);
                    p_squ = page_sequence.Text;
                    tlb_check = (Boolean)check_tlb.EditValue;
                    if (tlb_check)
                    {
                        time_tlb = int.Parse(tlb_time.Text);
                        num_tlb = int.Parse(tlb_num.Text);

                    }
                    else
                    {
                        time_tlb = 0;
                        num_tlb = 0;
                    }
                     run_thread = new Thread(new ThreadStart(run_page));
                    run_thread.Start();
                    timer1.Start();
                    run.Text = "暂停";
                    run_t = 1;
                      //run_page();
                }
           
            }
            else if (run_t == 1)
            {
                if(thread_FIFO!=null)
                thread_FIFO.Suspend();
                if (thread_LRU != null)
                thread_LRU.Suspend();
                if (thread_LFU != null)
                thread_LFU.Suspend();
                if (thread_OPT != null)
                thread_OPT.Suspend();

                run.Text = "继续";
                run_t = 2;
            }
            else
            {
                if (thread_FIFO != null)
                thread_FIFO.Resume();
                if (thread_LRU != null)
                thread_LRU.Resume();
                if (thread_LFU != null)
                thread_LFU.Resume();
                if (thread_OPT != null)
                thread_OPT.Resume();
                run.Text = "暂停";
                run_t = 1;
            }
        }
      void  Write(string filename)
        {

                FileStream fs = new FileStream(filename, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(num_page_frame.ToString());
                sw.Write("\r\n");
                sw.Write(num_page.ToString());
                sw.Write("\r\n");
                sw.Write(time_memory.ToString());
                sw.Write("\r\n");
                sw.Write(time_break.ToString());
                sw.Write("\r\n");
                sw.Write(p_squ);
                sw.Write("\r\n");
                sw.Write(tlb_check.ToString());
                sw.Write("\r\n");
                sw.Write(num_tlb.ToString());
                sw.Write("\r\n");
                sw.Write(time_tlb.ToString());
                sw.Write("\r\n");
                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                fs.Close();

        }
        private void 页面置换_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult res = MessageBox.Show("是否确定保存！", "操作提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (res == System.Windows.Forms.DialogResult.Yes)
            {
               Write("record.txt");

            }
            thread_FIFO.Abort();
            thread_LFU.Abort();
            thread_LRU.Abort();
            thread_OPT.Abort();
            thread_FIFO.Join();
            thread_LRU.Join();
            thread_LFU.Join();
            thread_OPT.Join();
            run_thread.Abort();
            run_thread.Join();
        }

        private void check_tlb_CheckedChanged(object sender, EventArgs e)
        {
            Boolean s = check_tlb.Checked;
            if (!s)
            {
                tlb_time.Text = "";
                tlb_num.Text = "";
            }
            tlb_num.Enabled = s;
            tlb_time.Enabled = s;
           
        }

        private void write_data_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "(*.txt)|*.txt";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;
            //显示
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fName = saveFileDialog.FileName;
                Write(fName);
            }

            //得到选择的路径
           // MessageBox.Show(saveFileDialog.FileName.ToString());
        }

        private void load_data_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();

            op.RestoreDirectory = true;
            op.Filter = "(*.txt)|*.txt";
            if (op.ShowDialog() == DialogResult.OK)
            read(op.FileName.ToString());
          //  MessageBox.Show(op.FileName.ToString());



        }

    }

}
       
