using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Prise.IntegrationTestsContract;
using Prise.IntegrationTestsHost.Models;
using Prise.IntegrationTestsHost.PluginLoaders;

namespace Prise.IntegrationTestsHost.Controllers
{
    public abstract class CalculationControllerBase : ControllerBase
    {
        protected ICalculationPluginLoader pluginLoader;
        public CalculationControllerBase(ICalculationPluginLoader pluginLoader)
        {
            this.pluginLoader = pluginLoader;
        }

        protected virtual async Task<ICalculationPlugin> GetCalculationPlugin()
        {
            return await this.pluginLoader.GetPlugin();
        }

        protected async Task<CalculationResponseModel> Calculate(CalculationRequestModel requestModel)
        {
            // The plugin is eagerly loaded (in-scope)
            return new CalculationResponseModel
            {
                Result = (await GetCalculationPlugin()).Calculate(requestModel.A, requestModel.B)
            };
        }

        protected async Task<CalculationResponseModel> CalculateInt(CalculationRequestModel requestModel)
        {
            // Overloading works due to matching the Proxy on parameter count and types
            return new CalculationResponseModel
            {
                Result = (await GetCalculationPlugin()).Calculate((int)requestModel.A, (int)requestModel.B)
            };
        }

        protected async Task<CalculationResponseModel> CalculateComplex(CalculationRequestModel requestModel)
        {
            // Complex parameters are serialized across Application Domains using Newtonsoft JSON
            var context = new CalculationContext
            {
                A = requestModel.A,
                B = requestModel.B
            };
            return new CalculationResponseModel
            {
                Result = (await GetCalculationPlugin()).CalculateComplex(context)
            };
        }

        protected async Task<CalculationResponseModel> CalculateComplexOutput(CalculationRequestModel requestModel)
        {
            var context = new CalculationContext
            {
                A = requestModel.A,
                B = requestModel.B
            };
            // Complex results are dezerialized using XML Deserialization (by default)
            return new CalculationResponseModel
            {
                Result = (await GetCalculationPlugin()).CalculateComplexResult(context).Result
            };
        }

        protected async Task<CalculationResponseModel> CalculateMultiple(CalculationRequestMultiModel requestModel)
        {
            // Ever more complex objects are serialized correctly
            var calculationContext = new ComplexCalculationContext
            {
                Calculations = requestModel.Calculations.Select(c => new CalculationContext { A = c.A, B = c.B }).ToArray()
            };

            return new CalculationResponseModel
            {
                Result = (await GetCalculationPlugin()).CalculateMutiple(calculationContext).Results.Sum(r => r.Result)
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
                Result = (await (await GetCalculationPlugin()).CalculateMutipleAsync(calculationContext)).Results.Sum(r => r.Result)
            };
        }
    }
}
