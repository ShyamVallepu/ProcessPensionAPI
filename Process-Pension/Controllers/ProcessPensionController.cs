using System;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProcessPension.Provider;
using ProcessPension.Repository;

namespace ProcessPension.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessPensionController : ControllerBase
    {
        static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(ProcessPensionController));
       
        private IProcessRepo _repo;
        /// <summary>
        /// Dependency Injection
        /// </summary>
        /// <param name="repo"></param>
        public ProcessPensionController(IProcessRepo repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// 1. This method is taking values given by MVC Client i.e. Pension Management Portal as Parameter
        /// 2. Calling the Pension Detail Microservice and checking all the values
        /// 3. Calling the Pension Disbursement Microservice to get the Status Code
        /// </summary>
        /// <param name="processPensionInput"></param>
        /// <returns>Details to be displayed on the MVC Client</returns>
        [Route("[action]")]
        [HttpPost]
        public IActionResult ProcessPension(PensionerInput pensionerInput)
        {
            _log.Info("Pensioner details invoked from Client Input");
            PensionerInput Input = new PensionerInput();
            Input.AadhaarNumber = pensionerInput.AadhaarNumber;

            //repo = new ProcessRepo(pro);
            PensionDetail pensionDetail = _repo.GetClientInfo(Input.AadhaarNumber);

            if (pensionDetail == null)
            {
                PensionDetail mvc = new PensionDetail();
                mvc.Name = "";
                mvc.Pan = "";
                mvc.PensionAmount = 0;
                mvc.DateOfBirth = new DateTime(2000, 01, 01);
                mvc.BankType = 1;
                mvc.AadharNumber = "***";
                mvc.Status = 20;

                return NotFound();
            }



            double pensionAmount;

            ValueforCalCulation pensionerInfo = _repo.GetCalculationValues(Input.AadhaarNumber);
            pensionAmount = CalculatePensionAmount(pensionerInfo.SalaryEarned, pensionerInfo.Allowances, pensionerInfo.BankType, pensionerInfo.PensionType);

            PensionDetail mvcClientOutput = new PensionDetail();

            if (Input.AadhaarNumber.Equals(pensionDetail.AadharNumber))
            {
                mvcClientOutput.Name = pensionDetail.Name;
                mvcClientOutput.Pan = pensionDetail.Pan;
                mvcClientOutput.PensionAmount = pensionAmount;
                mvcClientOutput.DateOfBirth = pensionDetail.DateOfBirth.Date;
                mvcClientOutput.PensionType = pensionerInfo.PensionType;
                mvcClientOutput.BankType = pensionerInfo.BankType;
                mvcClientOutput.AadharNumber = pensionDetail.AadharNumber;
                mvcClientOutput.Status = 20;
            }
            else
            {
                mvcClientOutput.Name = "";
                mvcClientOutput.Pan = "";
                mvcClientOutput.PensionAmount = 0;
                mvcClientOutput.DateOfBirth = new DateTime(2000, 01, 01);
                mvcClientOutput.PensionType = pensionerInfo.PensionType;
                mvcClientOutput.BankType = 1;
                mvcClientOutput.AadharNumber = "****";
                mvcClientOutput.Status = 21;

                //return mvcClientOutput;
            }

            ProcessPensionInput Client = new ProcessPensionInput();
            Client.PensionAmount = mvcClientOutput.PensionAmount;
            if (mvcClientOutput.BankType == 1)
            {
                Client.BankCharge = 500;
            }
            else
            {
                Client.BankCharge = 550;
            }

            return Ok(Client);
            //return OkObjectResult(Client);

           
        }

        private double CalculatePensionAmount(int salary, int allowances,int bankType , PensionType pensionType)
        {
            double pensionAmount;

            //repo = new ProcessRepo(pro);

            pensionAmount = _repo.CalcPensionAmount(salary, allowances, bankType, pensionType);

            return pensionAmount;
        }

    }

}
