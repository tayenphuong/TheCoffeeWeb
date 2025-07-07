using System.Collections.Generic;
using System.Web;
using TheCoffee.Models.ViewModel;


public static class CartSessionHelper
{
    private const string CartSessionKey = "CartSession";

    public static List<CartItemViewModel> GetCart()
    {
        var session = HttpContext.Current.Session;
        if (session[CartSessionKey] == null)
        {
            session[CartSessionKey] = new List<CartItemViewModel>();
        }

        return (List<CartItemViewModel>)session[CartSessionKey];
    }

    public static void SaveCart(List<CartItemViewModel> cart)
    {
        HttpContext.Current.Session[CartSessionKey] = cart;
    }

    public static void ClearCart()
    {
        HttpContext.Current.Session[CartSessionKey] = null;
    }
}
