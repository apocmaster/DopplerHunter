using System;
using System.Collections.Generic;
using System.Text;

namespace DopplerHunter.Events
{
    public class HashesCalculatedEventArgs(int hashesCalculated)
    {
        public int HasesCalculated { get; } = hashesCalculated;
    }
}
