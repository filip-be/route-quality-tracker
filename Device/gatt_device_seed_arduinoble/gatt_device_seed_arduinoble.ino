#include <ArduinoBLE.h>

#define BUTTON_USER                 5
#define BUTTON_PRESS_DELAY          100
#define BLE_APPERANCE_GATT_SERVICE  0x1801
#define BLE_APPERANCE_GATT_SERVICE_BATTERY_SERVICE  0x180F

const uint8_t LEDS_ARRAY[3] = { LED_RED, LED_BLUE, LED_GREEN };

// Generic Attribute Service
BLEService gattService("1801");
// Positioning Characteristics
BLEUnsignedCharCharacteristic positioningCharacteristic("0x2A69",
  BLERead | BLENotify);

// Battery service
BLEService batteryService("180F");
// Battery level characteristics
BLEUnsignedCharCharacteristic batteryLevelChar("2A19",  // standard 16-bit characteristic UUID
  BLERead | BLENotify); // remote clients will be able to get notifications if this characteristic changes

uint8_t batteryLevel = 100;

void lightLed(int ledPin) {
  for(int i = 0; i < 3; i++) {
    uint8_t pinValue = ledPin == LEDS_ARRAY[i] ? LOW : HIGH;
    
    digitalWrite(LEDS_ARRAY[i], pinValue);
  }
}

void serialPrint(const char* text) {
  if (Serial) {
      Serial.println(text);
  }
}

void waitForButtonPress() {
  serialPrint("waiting for button to be pressed...");
  delay(BUTTON_PRESS_DELAY);
  while (digitalRead(BUTTON_USER) == HIGH) {}
  while (digitalRead(BUTTON_USER) == LOW) {}
}

void setStatusAndWaitForButtonPress(const uint8_t ledPin) {
  BLEDevice central = BLE.central();
  while (!central || !central.connected()) {
    serialPrint("Waiting for connection...");

    delay(1000);
    lightLed(ledPin);
    delay(1000);
    lightLed(-1);
    if (!central) {
      serialPrint("Central is not connected");
      central = BLE.central();
    }
  }


  switch (ledPin) {
    case LED_RED:
      serialPrint("Setting status to Bad");
      positioningCharacteristic.writeValue(10);
      break;
    case LED_BLUE:
      serialPrint("Setting status to Standard");
      positioningCharacteristic.writeValue(20);
      break;
    case LED_GREEN:
      serialPrint("Setting status to Good");
      positioningCharacteristic.writeValue(30);
      break;
    default:
      serialPrint("unknown status");
      return;
  }

  lightLed(ledPin);
  
  char buffer[50];
  sprintf(buffer, "Setting battery status to: %u", batteryLevel);
  serialPrint(buffer);
  batteryLevelChar.writeValue(batteryLevel);
  batteryLevel--;
  
  waitForButtonPress();
}

// Setup device
void setup() {
  delay(500);
  if (Serial) {
    Serial.begin(115200);
    delay(500);
  }

  // configure IO
  pinMode(LED_RED, OUTPUT);
  pinMode(LED_BLUE, OUTPUT);
  pinMode(LED_GREEN, OUTPUT);

  pinMode(BUTTON_USER, INPUT_PULLUP);

  // stop LED
  lightLed(-1);

  // initialize BLE
  if (!BLE.begin()) {
    serialPrint("Starting BLE failed!");

    while (1);
  }

  BLE.setLocalName("QualityTracker");
  // Bluefruit.Advertising.clearData();

  // Bluefruit.setTxPower(4);


  // Start BLE advertisement
  startAdvertising();
}

void startAdvertising()
{
  // Advertising packet
  // Bluefruit.Advertising.addFlags(BLE_GAP_ADV_FLAGS_LE_ONLY_GENERAL_DISC_MODE);
  // Bluefruit.Advertising.addTxPower();
  // Bluefruit.Advertising.addAppearance(BLE_APPERANCE_GATT_SERVICE);

  // Include General GATT Service
  BLE.setAdvertisedService(gattService);
  gattService.addCharacteristic(positioningCharacteristic);
  BLE.addService(gattService);

  // Start advertising
  BLE.setConnectable(true);
  BLE.advertise();
  serialPrint("Bluetooth® device active, waiting for connections...");
}

// Loop function
void loop() {
  setStatusAndWaitForButtonPress(LED_BLUE);
  
  setStatusAndWaitForButtonPress(LED_GREEN);

  setStatusAndWaitForButtonPress(LED_BLUE);
  
  setStatusAndWaitForButtonPress(LED_RED);
}
