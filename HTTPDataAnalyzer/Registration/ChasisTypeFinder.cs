using System;
using System.Management;

namespace HTTPDataAnalyzer.Registration
{
    public enum ChassisTypes
    {
        Other = 1,
        Unknown,
        Desktop,
        LowProfileDesktop,
        PizzaBox,
        MiniTower,
        Tower,
        Portable,
        Laptop,
        Notebook,
        Handheld,
        DockingStation,
        AllInOne,
        SubNotebook,
        SpaceSaving,
        LunchBox,
        MainSystemChassis,
        ExpansionChassis,
        SubChassis,
        BusExpansionChassis,
        PeripheralChassis,
        StorageChassis,
        RackMountChassis,
        SealedCasePC,
        VirutalBox
    }

    class ChasisTypeFinder
    {
        public static ChassisTypes GetChassisType()
        {
            ChassisTypes cType = ChassisTypes.Unknown;
            ManagementClass systemEnclosures = new ManagementClass("Win32_SystemEnclosure");
            foreach (ManagementObject obj in systemEnclosures.GetInstances())
            {
                foreach (int i in (UInt16[])(obj["ChassisTypes"]))
                {
                    if (i > 0 && i < 25)
                    {
                        cType = (ChassisTypes)i;
                    }
                }
            }

            if (cType == ChassisTypes.Other)
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
                {
                    using (var items = searcher.Get())
                    {
                        foreach (var item in items)
                        {
                            string manufacturer = item["Manufacturer"].ToString().ToLower();
                            if ((item["Model"].ToString().ToUpperInvariant().Contains("VIRTUAL"))
                                || manufacturer.Contains("vmware")
                                || item["Model"].ToString() == "VirtualBox")
                            {
                                cType = ChassisTypes.VirutalBox;
                            }
                        }
                    }
                }
            }

            return cType;
        }
    }
}
