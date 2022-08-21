using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProcessPension.Provider
{
    public interface IPensionProvider
    {
        public HttpResponseMessage PensionDetail(string aadhar);

        public PensionDetail GetClientInfo(string aadhar);

        public ValueforCalCulation GetCalculationValues(string aadhar);

        public double CalcPensionAmount(int salary, int allowances, int bankType, PensionType pensionType);
    }
}
