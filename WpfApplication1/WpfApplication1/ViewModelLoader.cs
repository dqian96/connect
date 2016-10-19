using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    class ViewModelLoader
    {
        static ViewModel viewModelStatic;

        public ViewModelLoader()
        {
        }

        public static ViewModel ViewModelStatic
        {
            get
            {
                if (viewModelStatic == null)
                {
                    viewModelStatic = new ViewModel();
                }
                return viewModelStatic;
            }
        }

        public ViewModel ViewModel
        {
            get
            {
                return ViewModelStatic;
            }
        }

        public static void Cleanup()
        {
            if (viewModelStatic != null)
            {
                viewModelStatic.Cleanup();
            }
        }
    }
}
