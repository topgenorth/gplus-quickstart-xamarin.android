using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace com.xamarin.googleplus.quickstart
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/ic_launcher")]
    public class MainActivity : ListActivity
    {
        static readonly string[] _examples = { "Google" };
        ArrayAdapter<string> _menuAdapter;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            _menuAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, _examples);
            ListAdapter = _menuAdapter;
        }

        protected override void OnListItemClick(ListView l, View v, int position, long id)
        {
            base.OnListItemClick(l, v, position, id);

            switch (position)
            {
                case 0:
                    Intent intent = new Intent(this, typeof(GoogleExample));
                    StartActivity(intent);
                    break;
                default:
                    Toast.MakeText(this, "Don't know how to handle item " + position, ToastLength.Short).Show();
                    break;
            }
        }
    }
}
