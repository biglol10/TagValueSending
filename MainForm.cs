using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CommonUtils;
using log4net;
using System.Timers;
using Uniformance.PHD;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;

namespace PHDClient
{

    public partial class MainWindow : Form
    {

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        System.Timers.Timer aTimer = new System.Timers.Timer(30000);


        ////////////////////////////////////////////////    ---- START ----    ////////////////////////////////////////////////
        // Used Entity Framework

        public MainWindow()
        {

            InitializeComponent();
            InitializeForm();
        }


        private void SourceFacChanged(object sender, EventArgs e)   // Combobox의 값이 바뀔 때마다 기존에 표시되던 list 목록을 바꿈
        {
            using (totPISEntities4 db = new totPISEntities4())
            {
                if (source_combobox.SelectedItem.ToString() == "전체")
                {
                    dataGridView1.DataSource = null;
                    var lists = db.SERVER_CONNECTION.Where(p=>p.IN_USE=="Y").Select(p => new
                    {
                        SOURCE_SERVER = p.SOURCE_SERVER,
                        SOURCE_TAG_NAME = p.SOURCE_TAG,
                        TARGET_SERVER = p.TARGET_SERVER,
                        TARGET_TAG_NAME = p.TARGET_TAG,
                        TIME = p.TIME_CODE
                    });
                    dataGridView1.DataSource = lists.ToList();
                }
                else
                {
                    dataGridView1.DataSource = null;
                    var lists = db.SERVER_CONNECTION.Where(p =>p.IN_USE=="Y" && p.SOURCE_SERVER == source_combobox.SelectedItem.ToString()).Select(p => new
                    {
                        SOURCE_SERVER = p.SOURCE_SERVER,
                        SOURCE_TAG_NAME = p.SOURCE_TAG,
                        TARGET_SERVER = p.TARGET_SERVER,
                        TARGET_TAG_NAME = p.TARGET_TAG,
                        TIME = p.TIME_CODE
                    });
                    dataGridView1.DataSource = lists.ToList();
                }
            }
        }


        public void InitializeForm()           // 기존에 각 batch program에 표시되었던 리스트를 한번에 다 표시
        {
            source_combobox.Items.Clear();
            source_combobox.Items.Add("전체");
            using (totPISEntities4 db = new totPISEntities4())
            {

                var factoryItems = db.SERVER_CONNECTION.Where(p=>p.IN_USE=="Y").Select(p => new { p.SOURCE_SERVER }).Distinct().OrderBy(p => p.SOURCE_SERVER).Select(p => p.SOURCE_SERVER);
                foreach (var item in factoryItems)
                    source_combobox.Items.Add(item);  // combobox에 넣기
                source_combobox.SelectedIndex = 0;

                var lists = db.SERVER_CONNECTION.Where(p => p.IN_USE == "Y").Select(p => new
                {
                    SOURCE_SERVER = p.SOURCE_SERVER,
                    SOURCE_TAG_NAME = p.SOURCE_TAG,
                    TARGET_SERVER = p.TARGET_SERVER,
                    TARGET_TAG_NAME = p.TARGET_TAG,
                    TIME = p.TIME_CODE.Substring(1)
                });

                dataGridView1.DataSource = lists.ToList();
            }
        }

        private void alterButton(object sender, EventArgs e)
        {
            int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;

            DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];

            string source_server = Convert.ToString(selectedRow.Cells["SOURCE_SERVER"].Value);
            string source_tag = Convert.ToString(selectedRow.Cells["SOURCE_TAG_NAME"].Value);
            string target_server = Convert.ToString(selectedRow.Cells["TARGET_SERVER"].Value);
            string target_tag = Convert.ToString(selectedRow.Cells["TARGET_TAG_NAME"].Value);
            string time = Convert.ToString(selectedRow.Cells["TIME"].Value);

            alterPopUp popup = new alterPopUp(source_server, source_tag, target_server, target_tag, time, this, source_combobox.SelectedIndex);
            popup.Show();
        }


        private void addButton(object sender, EventArgs e)
        {
            addPopUp popup = new addPopUp(this, source_combobox.SelectedIndex);
            popup.Show();
        }


        //private void T10_Click(object sender, EventArgs e)
        //{
        //    Process.Start(@"C:\Users\Administrator\Desktop\통합 Input 프로그램\This Program\Debug - 통합\ManulInput_Combined.exe", "10");
        //}

        //private void T30_Click(object sender, EventArgs e)
        //{
        //    Process.Start(@"C:\Users\Administrator\Desktop\통합 Input 프로그램\This Program\Debug - 통합\ManulInput_Combined.exe", "30");
        //}

        //private void T60_Click(object sender, EventArgs e)
        //{
        //    Process.Start(@"C:\Users\Administrator\Desktop\통합 Input 프로그램\This Program\Debug - 통합\ManulInput_Combined.exe", "60");
        //}

        private void other_Click(object sender, EventArgs e)
        {
            //Process.Start(@"C:\Users\Administrator\Desktop\통합 Input 프로그램\This Program\Debug - 통합\ManulInput_Combined.exe", timeValues.SelectedItem.ToString().Substring(1));
            Process.Start(@"C:\Users\LG\Desktop\TASK\PIS\Practice\Overall\Manualinput 전공장 통합 (THIS)\PHDClient\bin\Debug\ManulInput_Combined.exe", timeValues.SelectedItem.ToString().Substring(1));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (totPISEntities4 db = new totPISEntities4())
            {
                MessageBox.Show(db.SERVER_CONNECTION.Where(p => p.ID == 100).ToList()[0].SOURCE_TAG);
            }
        }


    }
}

