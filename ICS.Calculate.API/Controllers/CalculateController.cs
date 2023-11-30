using ICS.Calculate.API.Util;
using ICS.Models.Builders;
using ICS.Models.Enumerators;
using ICS.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Globalization;

namespace ICS.Calculate.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculateController : Controller
    {
        private string _collectorAPIHost;
        private int _collectorAPIPort;

        public CalculateController()
        {
            _collectorAPIHost = ComnunicationSettings.CollectorHost;
            _collectorAPIPort = ComnunicationSettings.CollectorPort;
        }

        [HttpPost("comparescenarios")]
        [AllowAnonymous]
        public async Task<ActionResult> CompareScenario([FromBody] InvestimentsData scenarios)
        {
            try
            {
                Debug.WriteLine($"{JsonConvert.SerializeObject(scenarios)}");

                HttpClient request = new HttpClientBuilder().Host($"http://{_collectorAPIHost}").Port(_collectorAPIPort).Build();
                var responseBCB = await request.GetAsync("collector/lastannualizedselic252");

                var statusCode = responseBCB.StatusCode;
                if (statusCode != System.Net.HttpStatusCode.OK)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, TxTResponses.GetTxTResponse(TxTResponse.Failure_GetIndicator));
                }

                Selic selic = JsonConvert.DeserializeObject<Selic>(JToken.Parse(responseBCB.Content.ReadAsStringAsync().Result).ToString());

                var responseIBGE = await request.GetAsync("collector/lastannualavarageipca");
                statusCode = responseIBGE.StatusCode;

                if (statusCode != System.Net.HttpStatusCode.OK)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, TxTResponses.GetTxTResponse(TxTResponse.Failure_GetIndicator));
                }

                Ipca ipcaData = JsonConvert.DeserializeObject<Ipca>(JToken.Parse(responseIBGE.Content.ReadAsStringAsync().Result).ToString());

                double cdi = double.Parse(selic.valor, CultureInfo.InvariantCulture);
                double ipca = double.Parse(ipcaData.V, CultureInfo.InvariantCulture);
                double calc1, calc2, calc3;
                double finalAmount1 = 0.0, finalAmount2 = 0.0, finalAmount3 = 0.0;

                string scenario1Name = scenarios?.stage1?.investment ?? "";
                string scenario2Name = scenarios?.stage2?.investment ?? "";
                string scenario3Name = scenarios?.stage3?.investment ?? "";


                int days = Util.Calculate.InvestimentDays(scenarios.deadline);

                if (scenarios.stage1 != null)
                {
                    double fees1 = Util.Calculate.GetResultFees(scenarios.stage1.inputYield, scenarios.stage1.annualAdministrationFee);
                    calc1 = Util.Calculate.GetProfitability(fees1, scenarios.stage1.investment.ToLower(), scenarios.stage1.profitability, cdi, ipca);
                    finalAmount1 =
                        scenarios.stage1.investment == "lci" ?
                        Util.Calculate.CalculateFinalAmount(days, scenarios.amount, calc1, false) :
                        Util.Calculate.CalculateFinalAmount(days, scenarios.amount, calc1, true);
                }

                if (scenarios.stage2 != null)
                {
                    double fees2 = Util.Calculate.GetResultFees(scenarios.stage2.inputYield, scenarios.stage2.annualAdministrationFee);
                    calc2 = Util.Calculate.GetProfitability(fees2, scenarios.stage2.investment.ToLower(), scenarios.stage2.profitability, cdi, ipca);
                    finalAmount2 =
                        scenarios.stage2.investment == "lci" ?
                        Util.Calculate.CalculateFinalAmount(days, scenarios.amount, calc2, false) :
                        Util.Calculate.CalculateFinalAmount(days, scenarios.amount, calc2, true);
                }

                if (scenarios.stage3 != null)
                {
                    double fees3 = Util.Calculate.GetResultFees(scenarios.stage3.inputYield, scenarios.stage3.annualAdministrationFee);
                    calc3 = Util.Calculate.GetProfitability(fees3, scenarios.stage3.investment.ToLower(), scenarios.stage3.profitability, cdi, ipca);
                    finalAmount3 =
                        scenarios.stage3.investment == "lci" ?
                        Util.Calculate.CalculateFinalAmount(days, scenarios.amount, calc3, false) :
                        Util.Calculate.CalculateFinalAmount(days, scenarios.amount, calc3, true);
                }

                JArray dataList = new JArray();
                //JArray calc1List = new JArray();
                //JArray calc2List = new JArray();
                //JArray calc3List = new JArray();

                if (finalAmount1 > 0)
                {
                    //calc1List.Add(finalAmount1);
                    JObject scenario1 = new JObject() {
                        new JProperty("name", scenario1Name), new JProperty("data", finalAmount1)
                    };
                    dataList.Add(scenario1);
                }

                if (finalAmount2 > 0)
                {
                    //calc2List.Add(finalAmount2);
                    JObject scenario2 = new JObject() {
                        new JProperty("name", scenario2Name), new JProperty("data", finalAmount2)
                    };
                    dataList.Add(scenario2);
                }

                if (finalAmount3 > 0)
                {
                    //calc3List.Add(finalAmount3);
                    JObject scenario3 = new JObject() {
                        new JProperty("name", scenario3Name), new JProperty("data", finalAmount3)
                    };
                    dataList.Add(scenario3);
                }

                JObject series = new JObject() { new JProperty("series", dataList) };

                return Ok(JsonConvert.SerializeObject(series));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
        

        [HttpGet("realprofitability")]
        [AllowAnonymous]
        public async Task<ActionResult> GetRealProfitability()
        {
            try
            {
                HttpClient request = new HttpClientBuilder().Host($"http://{_collectorAPIHost}").Port(_collectorAPIPort).Build();
                var responseCollector = await request.GetAsync("collector/ipcatotal");

                var statusCode = responseCollector.StatusCode;
                if (statusCode != System.Net.HttpStatusCode.OK)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, TxTResponses.GetTxTResponse(TxTResponse.Failure_GetIndicator));
                }

                IpcaCalculated ipcaCalculated = JsonConvert.DeserializeObject<IpcaCalculated>(JToken.Parse(responseCollector.Content.ReadAsStringAsync().Result).ToString());

                responseCollector = await request.GetAsync("collector/bbinvestiments");
                statusCode = responseCollector.StatusCode;

                if (statusCode != System.Net.HttpStatusCode.OK)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, TxTResponses.GetTxTResponse(TxTResponse.Failure_GetIndicator));
                }

                Object bbInvestiments = JsonConvert.DeserializeObject<Object>(JToken.Parse(responseCollector.Content.ReadAsStringAsync().Result).ToString());


                return Ok(JsonConvert.SerializeObject(ipcaCalculated));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }


    }
}
