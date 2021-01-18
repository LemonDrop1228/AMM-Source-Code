using System;
using System.Collections.Generic;
using System.Text;

namespace SerializableModinfo
{
    public class Dlc
    {
        public Dlc() { }
        //use: "Anarchist", "SunkenTreasures", "Botanica", "ThePassage", "SeatOfPower", "BrightHarvest", "LandOfLions", "Christmas", "AmusementPark, "CityLife"
        public String DLC { get; set; }
        //use: "required", "partly", "atLeastOneRequired"
        public String Dependant { get; set; }
    }
}
