using Microsoft.AspNetCore.Mvc;

namespace Market.Web.Authorization
{
    // Używamy TypeFilterAttribute, aby móc wstrzykiwać DbContext do Filtra
    public class BuyerAttribute : TypeFilterAttribute
    {
        public BuyerAttribute() : base(typeof(BuyerFilter))
        {
        }
    }
}