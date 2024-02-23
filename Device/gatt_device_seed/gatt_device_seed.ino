#include <bluefruit.h>

#define BUTTON_USER                 5
#define BUTTON_PRESS_DELAY          100
#define BLE_APPERANCE_GATT_SERVICE  0x1801

const uint8_t LEDS_ARRAY[3] = { LED_RED, LED_BLUE, LED_GREEN };

BLEDis bledis;
BLEService bleService(BLEUuid(BLE_APPERANCE_GATT_SERVICE));
BLECharacteristic positioningCharacteristic(BLEUuid(0x2A69), CharsProperties::CHR_PROPS_NOTIFY | CharsProperties::CHR_PROPS_READ);

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
  while (Bluefruit.connected() == 0) {
    serialPrint("Waiting for connection...");

    delay(1000);
    lightLed(ledPin);
    delay(1000);
    lightLed(-1);
  }


  switch (ledPin) {
    case LED_RED:
      serialPrint("Setting status to Bad");
      positioningCharacteristic.notify8(10);
      break;
    case LED_BLUE:
      serialPrint("Setting status to Standard");
      positioningCharacteristic.notify8(20);
      break;
    case LED_GREEN:
      serialPrint("Setting status to Good");
      positioningCharacteristic.notify8(30);
      break;
    default:
      serialPrint("unknown status");
      return;
  }

  lightLed(ledPin);
  // serialPrint("Checking current characteristic value");
  // ble.sendCommandCheckOK("AT+GATTCHAR=1");
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
  Bluefruit.begin();
  
  Bluefruit.Advertising.clearData();

  Bluefruit.setTxPower(4);


  // Configure Device Information Service
  bledis.setManufacturer("Cindalnet");
  bledis.setModel("Route Quality Tracking Device");
  bledis.begin();

  // Configure General GATT Service
  bleService.begin();

  // Start BLE advertisement
  startAdvertising();
}

void startAdvertising()
{
  // Advertising packet
  Bluefruit.Advertising.addFlags(BLE_GAP_ADV_FLAGS_LE_ONLY_GENERAL_DISC_MODE);
  Bluefruit.Advertising.addTxPower();
  Bluefruit.Advertising.addAppearance(BLE_APPERANCE_GATT_SERVICE);
  
  // Include General GATT Service
  Bluefruit.Advertising.addService(bleService);
  
  // There is enough room for the dev name in the advertising packet
  Bluefruit.setName("QualityTracker");
  Bluefruit.Advertising.addName();

  // Define characteristic
  positioningCharacteristic.setPermission(SECMODE_OPEN, SECMODE_NO_ACCESS);
  positioningCharacteristic.setFixedLen(1);
  positioningCharacteristic.begin();

    /* Start Advertising
   * - Enable auto advertising if disconnected
   * - Interval:  fast mode = 20 ms, slow mode = 152.5 ms
   * - Timeout for fast mode is 30 seconds
   * - Start(timeout) with timeout = 0 will advertise forever (until connected)
   * 
   * For recommended advertising interval
   * https://developer.apple.com/library/content/qa/qa1931/_index.html   
   */
  Bluefruit.Advertising.restartOnDisconnect(true);
  Bluefruit.Advertising.setInterval(32, 244);    // in unit of 0.625 ms
  Bluefruit.Advertising.setFastTimeout(30);      // number of seconds in fast mode
  Bluefruit.Advertising.start(0);                // 0 = Don't stop advertising after n seconds

}

// Loop function
void loop() {
  setStatusAndWaitForButtonPress(LED_BLUE);
  
  setStatusAndWaitForButtonPress(LED_GREEN);

  setStatusAndWaitForButtonPress(LED_BLUE);
  
  setStatusAndWaitForButtonPress(LED_RED);
}
