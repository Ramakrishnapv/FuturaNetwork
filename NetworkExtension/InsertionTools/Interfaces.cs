using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Futura.ArcGIS.NetworkExtension
{

    public interface IElectricPrimaryDeviceClass
    {
    }

    public interface IElectricDeviceBankClass
    {

    }

    public interface ICapacitorBankClass : IElectricDeviceBankClass, IElectricPrimaryDeviceClass
    {
    }

    public interface IFuseBankClass :  IElectricDeviceBankClass, IElectricPrimaryDeviceClass
    {
    }

    public interface IOpenPointClass : IElectricPrimaryDeviceClass, IElectricDeviceBankClass
    {
    }

    public interface IRecloserBankClass :  IElectricDeviceBankClass, IElectricPrimaryDeviceClass
    {
    }

    public interface IRegulatorBankClass :  IElectricPrimaryDeviceClass, IElectricDeviceBankClass
    {
    }

    public interface ISectionalizerBankClass : IElectricPrimaryDeviceClass, IElectricDeviceBankClass
    {
    }

    public interface IStepTransformerBankClass : IElectricPrimaryDeviceClass, IElectricDeviceBankClass
    {
    }

    public interface ISwitchBankClass : IElectricDeviceBankClass, IElectricPrimaryDeviceClass
    {
    }

    public interface ITransformerBankClass : IElectricDeviceBankClass
    {
    }
}
