## Managing SMS Eagles as Azure IOT devices

This is a guide to how to setup SMS Eagles as Azure IOT Hubs devices.

### 1. Add new device to Azure IOT Hub:

You can either do it directly in [Azure portal](https://portal.azure.com) or by using the Azure CLI with the [Azure IoT extension](https://github.com/Azure/azure-iot-cli-extension). Make sure you have provisioned an iot hub that you want to use and are logged in to the correct azure subscription (It can be done by using `az login`, or if you are already logged in and just want to check you can use `az account show`). Then you can create a new device with the following command:

```
az iot hub device-identity create --hub-name nrx-cbs-nyss-iothub-dev --device-id test-eagle-1
```

If you want to list all device-ids:
```ps
az iot hub device-identity list --hub-name nrx-cbs-nyss-iothub-dev | ConvertFrom-Json | Foreach DeviceId
```

### 2. Retrieve the connection string
```
az iot hub device-identity show-connection-string --device-id test-eagle-1 --hub-name nrx-cbs-nyss-iothub-dev
```

### 3. Connect the SMS Eagle to the hub
We assume you want to connect an SMS Eagle to the Hub with the python script that we have made. This can be found in the [nyss-sms-gateway](https://github.com/nyss-platform-norcross/nyss-sms-gateway) repository. Clone the repo or download the two files needed:

- [nyssIotBridge.py](https://raw.githubusercontent.com/nyss-platform-norcross/nyss-sms-gateway/sms-eagle-iot-connection/SMSEagleIOTBridge/nyssIotBridge.py)
- [smsEagle-iot-hub-handler.py](https://raw.githubusercontent.com/nyss-platform-norcross/nyss-sms-gateway/sms-eagle-iot-connection/SMSEagleIOTBridge/smsEagle-iot-hub-handler.py)

Make sure the two files are in the same folder, and make sure you have python3 installed. Run it with the follwing arguments:

```python
python3 smsEagle-iot-hub-handler.py "IotHubConnectionString" "smseagleApiUserName" "smsEagleApiPwd"
```

...where the "IotHubConnectionString" is the connection string you retrieved in step 2. Then everyting should be ready to go.

### 4. Monitor events
```
az iot hub monitor-events --hub-name nrx-cbs-nyss-iothub-dev --output table
```