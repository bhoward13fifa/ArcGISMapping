using Caliburn.Micro;
using MappingDesktopUI.Event_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MappingDesktopUI.ViewModels
{
    public class ShellViewModel : Conductor<object>, IHandle<LogOnEvent>
    {
        private IEventAggregator _events;
        private MapViewModel _mapVM;

        public ShellViewModel(IEventAggregator events, MapViewModel workOrderVM)
        {
            _events = events;
            _mapVM = workOrderVM;

            _events.Subscribe(this);

            ActivateItem(IoC.Get<LoginViewModel>());
        }

        public void Handle(LogOnEvent message) 
        {
            ActivateItem(_mapVM);
        }
    }
}
