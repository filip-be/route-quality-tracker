using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteQualityTracker.Interfaces;

public interface IForegroundService
{
    bool ToggleService();

    event EventHandler OnServiceStart;

    event EventHandler OnServiceStop;
}