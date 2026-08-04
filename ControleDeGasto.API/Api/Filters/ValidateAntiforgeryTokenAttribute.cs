using Microsoft.AspNetCore.Mvc;

namespace ControleDeGasto.API.Api.Filters
{
    public class ValidateAntiforgeryTokenAttribute : ServiceFilterAttribute
    {
        public ValidateAntiforgeryTokenAttribute() : base(typeof(ValidateAntiforgeryTokenFilter))
        {
        }
    }
}
