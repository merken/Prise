using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Prise.IntegrationTestsContract;
using Prise.IntegrationTestsHost.Models;

namespace Prise.IntegrationTestsHost.Controllers
{
    public class CalculationControllerBase : ControllerBase
    {
        protected ICalculationPlugin _plugin;
        protected void SetPlugin(ICalculationPlugin plugin) => this._plugin = plugin;

        protected CalculationResponseModel Calculate(CalculationRequestModel requestModel)
        {
            // The plugin is eagerly loaded (in-scope)
            return new CalculationResponseModel
            {
                Result = _plugin.Calculate(requestModel.A, requestModel.B)
            };
        }

        protected CalculationResponseModel CalculateInt(CalculationRequestModel requestModel)
        {
            // Overloading works due to matching the Proxy on parameter count and types
            return new CalculationResponseModel
            {
                Result = _plugin.Calculate((int)requestModel.A, (int)requestModel.B)
            };
        }

        protected CalculationResponseModel CalculateComplex(CalculationRequestModel requestModel)
        {
            // Complex parameters are serialized across Application Domains using Newtonsoft JSON
            var context = new CalculationContext
            {
                A = requestModel.A,
                B = requestModel.B
            };
            return new CalculationResponseModel
            {
                Result = _plugin.CalculateComplex(context)
            };
        }

        protected CalculationResponseModel CalculateComplexOutput(CalculationRequestModel requestModel)
        {
            var context = new CalculationContext
            {
                A = requestModel.A,
                B = requestModel.B
            };
            // Complex results are dezerialized using XML Deserialization (by default)
            return new CalculationResponseModel
            {
                Result = _plugin.CalculateComplexResult(context).Result
            };
        }

        protected CalculationResponseModel CalculateMultiple(CalculationRequestMultiModel requestModel)
        {
            // Ever more complex objects are serialized correctly
            var calculationContext = new ComplexCalculationContext
            {
                Calculations = requestModel.Calculations.Select(c => new CalculationContext { A = c.A, B = c.B }).ToArray()
            };

            return new CalculationResponseModel
            {
                Result = _plugin.CalculateMutiple(calculationContext).Results.Sum(r => r.Result)
            };
        }

        protected async Task<CalculationResponseModel> CalculateMultipleAsync(CalculationRequestMultiModel requestModel)
        {
            // Ever more complex objects are serialized correctly
            var calculationContext = new ComplexCalculationContext
            {
                Calculations = requestModel.Calculations.Select(c => new CalculationContext { A = c.A, B = c.B }).ToArray()
            };

            return new CalculationResponseModel
            {
                Result = (await _plugin.CalculateMutipleAsync(calculationContext)).Results.Sum(r => r.Result)
            };
        }
    }
}
