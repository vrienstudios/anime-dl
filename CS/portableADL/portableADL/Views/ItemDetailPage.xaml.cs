using portableADL.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace portableADL.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}