using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ProcessPension.Provider
{
    public class PensionProvider : IPensionProvider
    {
        /// <summary>
        /// Dependency Injection
        /// </summary>
        
        static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(PensionProvider));
       
        /// <summary>
        /// Calling the Pension Detail Microservice
        /// </summary>
        /// <param name="aadhar"></param>
        /// <returns>value for calculations and client input</returns>
        public HttpResponseMessage PensionDetail(string aadhar)
        {
            PensionProvider banktype = new PensionProvider();
            
            HttpResponseMessage response = new HttpResponseMessage();
            string uriConn = "https://localhost:44391/";

            using (var client = new HttpClient())
            {
                client.BaseAddress =new Uri (uriConn);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    response = client.GetAsync("api/PensionerDetail/" + aadhar).Result;
                }
                catch(Exception e)
                {
                    _log.Error("Exception Occured" + e);
                    return null; 
                }
            }
            return response;
        }
        /// <summary>
        /// Getting the Values from Process Management Portal
        /// </summary>
        /// <param name="aadhar"></param>
        /// <returns></returns>
        public PensionDetail GetClientInfo(string aadhar)
        {
            PensionDetail Detail = new PensionDetail();
            HttpResponseMessage response = PensionDetail(aadhar);
            if (response == null)
            {
                Detail = null;
                return null;
            }
            string responseValue = response.Content.ReadAsStringAsync().Result;
            Detail = JsonConvert.DeserializeObject<PensionDetail>(responseValue);
            if(Detail == null)
            {
                return null;
            }
            return Detail;
        }
        /// <summary>
        /// Getting the values for calculation
        /// </summary>
        /// <param name="aadhar"></param>
        /// <returns>Values required for calculation</returns>
        public ValueforCalCulation GetCalculationValues(string aadhar)
        {
            PensionerDetail Detail = new PensionerDetail();
            HttpResponseMessage response = PensionDetail(aadhar);
            if (response == null)
            {
                Detail = null;
                return null;
            }
            string responseValue = response.Content.ReadAsStringAsync().Result;
            Detail = JsonConvert.DeserializeObject<PensionerDetail>(responseValue);

            ValueforCalCulation Values = new ValueforCalCulation()
            {
                SalaryEarned = Detail.SalaryEarned,
                Allowances = Detail.Allowances,
                BankType = (int)Detail.BankType,
                PensionType = (PensionType)Detail.PensionType
            };
            return Values;
        }

        /// <summary>
        /// Caluculates Pension Amount
        /// </summary>
        /// <param name="salary"></param>
        /// <param name="allowances"></param>
        /// <param name="bankType"></param>
        /// <param name="pensionType"></param>
        /// <returns></returns>
        public double CalcPensionAmount(int salary, int allowances, int bankType, PensionType pensionType)
        {
            double pensionAmount;
            if (pensionType == PensionType.Self)
                pensionAmount = (0.8 * salary) + allowances;
            else
                pensionAmount = (0.5 * salary) + allowances;

            if (bankType == 1)
                pensionAmount = pensionAmount + 500;
            else
                pensionAmount = pensionAmount + 550;

            return pensionAmount;
        }

    }
}
