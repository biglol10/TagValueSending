using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Uniformance.PHD;

namespace PHDClient
{
    class Methods
    {
        public Dictionary<string, string> ipList = new Dictionary<string, string>();        // 서버에 해당하는 IP
        public Dictionary<string, string> isNewServer = new Dictionary<string, string>();   // 서버가 320 버전인지 아닌지

        CPHDApi capi = new CPHDApi();   // 데이터 조회와 입력을 위해

        CPHDSetting source_setting = new CPHDSetting();
        CPHDSetting target_setting = new CPHDSetting();

        readonly object thisLock = new object();

        Tag tag = new Tag();

        List<ContainsValue> errorList = new List<ContainsValue>();

        string value;

        string srcfac = "";
        string srctag = "";
        string trgfac = "";
        string trgtag = "";


        public void InitializeMethodClass()
        {
            using (totPISEntities4 db = new totPISEntities4())
            {
                var getFacIP = from value in db.FACTORY_IP
                               select value;
                foreach (var item in getFacIP.ToList())
                {
                    isNewServer.Add(item.FACTORY, item.IS_320);     // server : O/X
                    ipList.Add(item.FACTORY, item.IP);              // server : IP
                }
                CPHDSetting source = new CPHDSetting();
                source.Tagname = "NCC_CABLE_TEMP";
                source.Hostname = ipList["NCC320"];
                source.StartDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                source.EndDate = source.StartDate;
                capi.FetchRowdata(source);
            }
        }


        public void DoSomething(List<Fac_Tag> sourceList, List<Fac_Tag> targetList, int seconds, DateTime Today)
        // 실행 과정: DoSomething(특정 유일한 시간에 해당하는 sourceList, 특정 유일한 시간에 해당하는 targetList, 유일 주기, 유일 주기에 해당하는 유일한 날짜)
        {
            while (true)       
            {
                try
                {
                    var time = DateTime.Now;
                    for (int i = 0; i < sourceList.Count; i++)
                    {
                        //int confidence = 0;
                        // source 설정하기
                        string sFac = "";
                        string sTag = "";
                        sFac = sourceList[i].Factory;
                        sTag = sourceList[i].Tag;
                        sTag = sTag.Replace("\r", "");
                        sTag = sTag.Replace("\n", "");
                        sourceList[i].Tag = sTag;

                        source_setting.Tagname = sourceList[i].Tag;
                        source_setting.Hostname = ipList[sourceList[i].Factory];
                        source_setting.UseRemoteAPI = isNewServer[sourceList[i].Factory.ToString()] == "O" ? true : false;
                        source_setting.StartDate = seconds == 10 ? time.ToString("yyyy-MM-dd HH:mm:ss") : Today.ToString("yyyy-MM-dd HH:mm:ss");
                        source_setting.EndDate = source_setting.StartDate;

                        target_setting.Tagname = targetList[i].Tag;
                        target_setting.Hostname = ipList[targetList[i].Factory];
                        target_setting.UseRemoteAPI = isNewServer[targetList[i].Factory.ToString()] == "O" ? true : false;
                        target_setting.StartDate = source_setting.StartDate;

                        try
                        {
                            srcfac = sFac;
                            srctag = sTag;
                            trgfac = targetList[i].Factory;
                            trgtag = target_setting.Tagname;
                            lock (thisLock)
                            {
                                if (sourceList[i].Type == "RAW")
                                {
                                    source_setting.SampleType = SampleReturnType.Raw;
                                    tag.TagName = source_setting.Tagname;
                                    var data = capi.FetchRowdataTag(source_setting, tag);
                                    if (data.Confidences[0] == 100 || data.Confidences[0] == 0)
                                    {
                                        value = data.DataValues[0].ToString();
                                    }
                                }
                                else
                                {
                                    source_setting.SampleType = SampleReturnType.Snapshot;
                                    source_setting.SampleFrequency = 60;
                                    var data = capi.FetchRowdata(source_setting);
                                    if(int.Parse(data.Tables[0].Rows[0]["Confidence"].ToString())==100 || int.Parse(data.Tables[0].Rows[0]["Confidence"].ToString()) == 0)
                                    {
                                        value = data.Tables[0].Rows[0]["Value"].ToString();
                                    }
                                }

                                if (targetList[i].Tag.IndexOf("ALM") != -1 && target_setting.Tagname != "MI_NCC_CABLE_ALM_NCC")
                                {
                                    if (value == "0")
                                        value = "OFF";
                                    else
                                        value = "ON";
                                }

                                capi.PutRowData(target_setting, value);
                            }
                        }
                        catch (Exception er)
                        {
                            errorList.Add(new ContainsValue { date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message = "Exception in entire method", source_server = srcfac, source_tag = srctag, target_server = trgfac, target_tag = trgtag });
                        }

                    }

                    if (seconds == 10)
                    {
                        Today = Today.AddSeconds(seconds);
                        Thread.Sleep(1);
                    }
                    else
                    {
                        var timeDiff = System.Math.Abs((Today - time).TotalSeconds);
                        var sleepTime = Convert.ToInt32(seconds * 1000 - timeDiff * 1000);
                        Today = Today.AddSeconds(seconds);   // 현재 시간에 초 단위로 설정된 주기를 더함
                        if (sleepTime <= 0)
                            Thread.Sleep(seconds);
                        else
                            Thread.Sleep(sleepTime);        // Thread 잠시 멈추기
                    }
                }
                catch (ThreadInterruptedException interrupted)
                {
                    break;
                }
                catch (NullReferenceException nullRef)
                {
                    errorList.Add(new ContainsValue { date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message = "NullRefException", source_server = srcfac, source_tag = srctag, target_server = trgfac, target_tag = trgtag });
                    continue;
                }
            }
        }


        public List<ContainsValue> reterror()
        {
            return errorList;
        }
    }
}


