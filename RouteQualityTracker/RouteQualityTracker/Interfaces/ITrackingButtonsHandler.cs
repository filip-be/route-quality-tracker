using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteQualityTracker.Interfaces;

public interface ITrackingButtonsHandler
{
    void OnButtonClick(string text);
}