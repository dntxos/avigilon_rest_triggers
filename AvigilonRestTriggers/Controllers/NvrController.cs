using AvigilonDotNet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AvigilonRestTriggers.Controllers
{

    [RoutePrefix("api/nvr")]
    public class NvrController : ApiController
    {
        public static AvigilonCli Avg = new AvigilonCli();
        // GET api/values
        public List<INvr> GetNvrs()
        {
            List<INvr> ret = new List<INvr>();

            try
            {
                var allnvrs = (Avg.GetAllNvrs());
                ret = allnvrs;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
            }
            return ret;
        }

        // GET api/values/5
        [Route("{host}")]
        public INvr Get(string host)
        {
            var nvr = Avg.GetNvr(host);
            return nvr;
        }

        // POST api/values
        public INvr Post([FromBody]string value)
        {
            var nvr = Avg.GetNvr(value);
            return nvr;
        }

        // PUT api/values/5
        [Route("{host}/trigger_alarm/{alarmName}")]
        [HttpGet][HttpPost]
        public void TriggerAlarm(string host, string alarmName)
        {
            var nvr = Avg.GetNvr(host);
            if (nvr != null)
            {
                foreach (var al in nvr.Alarms)
                {
                    if (al.Name.ToLower().Trim() == alarmName.ToLower().Trim())
                    {
                        al.TriggerAlarm(alarmName);
                    }

                }
            } else
            {
                throw new Exception("NVR is null");
            }

        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
