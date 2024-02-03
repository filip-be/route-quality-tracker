#include <Arduino.h>
#include <SPI.h>
#include "Adafruit_BLE.h"
#include "Adafruit_BluefruitLE_SPI.h"
#include "Adafruit_BluefruitLE_UART.h"

#include "BluefruitConfig.h"

#if SOFTWARE_SERIAL_AVAILABLE
  #include <SoftwareSerial.h>
#endif

/*=========================================================================
    APPLICATION SETTINGS
    -----------------------------------------------------------------------*/

    #define FACTORYRESET_ENABLE         1
    #define MINIMUM_FIRMWARE_VERSION    "0.6.6"


/*=========================================================================*/

#define LED_RED     12
#define LED_YELLOW  11
#define LED_GREEN   10
const uint8_t LEDS_ARRAY[3] = { LED_RED, LED_YELLOW, LED_GREEN };
#define BUTTON_USER 6

const int BUTTON_PRESS_DELAY = 100;

// Create the bluefruit object, either software serial...uncomment these lines
/*
SoftwareSerial bluefruitSS = SoftwareSerial(BLUEFRUIT_SWUART_TXD_PIN, BLUEFRUIT_SWUART_RXD_PIN);

Adafruit_BluefruitLE_UART ble(bluefruitSS, BLUEFRUIT_UART_MODE_PIN,
                      BLUEFRUIT_UART_CTS_PIN, BLUEFRUIT_UART_RTS_PIN);
*/

/* ...or hardware serial, which does not need the RTS/CTS pins. Uncomment this line */
// Adafruit_BluefruitLE_UART ble(BLUEFRUIT_HWSERIAL_NAME, BLUEFRUIT_UART_MODE_PIN);

/* ...hardware SPI, using SCK/MOSI/MISO hardware SPI pins and then user selected CS/IRQ/RST */
Adafruit_BluefruitLE_SPI ble(BLUEFRUIT_SPI_CS, BLUEFRUIT_SPI_IRQ, BLUEFRUIT_SPI_RST);

/* ...software SPI, using SCK/MOSI/MISO user-defined SPI pins and then user selected CS/IRQ/RST */
//Adafruit_BluefruitLE_SPI ble(BLUEFRUIT_SPI_SCK, BLUEFRUIT_SPI_MISO,
//                             BLUEFRUIT_SPI_MOSI, BLUEFRUIT_SPI_CS,
//                             BLUEFRUIT_SPI_IRQ, BLUEFRUIT_SPI_RST);

void lightLed(int ledPin) {
  for(int i = 0; i < 3; i++) {
    uint8_t pinValue = ledPin == LEDS_ARRAY[i] ? HIGH : LOW;
    
    digitalWrite(LEDS_ARRAY[i], pinValue);
  }
}

void error(const __FlashStringHelper*err) {
  Serial.println(err);
  while (1);
}

// the setup function runs once when you press reset or power the board
void setup() {
  while (!Serial) {};
  delay(500);

  Serial.begin(115200);

  
  Serial.println(F("Initializing button and LEDs..."));

  pinMode(LED_RED, OUTPUT);
  pinMode(LED_YELLOW, OUTPUT);
  pinMode(LED_GREEN, OUTPUT);

  pinMode(BUTTON_USER, INPUT_PULLUP);

  lightLed(-1);


  Serial.print(F("Initialising the Bluefruit LE module: "));

  if ( !ble.begin(VERBOSE_MODE) )
  {
    error(F("Couldn't find Bluefruit, make sure it's in CoMmanD mode & check wiring?"));
  }
  Serial.println( F("OK!") );

  if ( FACTORYRESET_ENABLE )
  {
    /* Perform a factory reset to make sure everything is in a known state */
    Serial.println(F("Performing a factory reset: "));
    if ( ! ble.factoryReset() ){
      error(F("Couldn't factory reset"));
    }
  }

  /* Disable command echo from Bluefruit */
  ble.echo(false);

  Serial.println("Requesting Bluefruit info:");
  /* Print Bluefruit information */
  ble.info();

  /* Change the device name to make it easier to find */
  Serial.println(F("Setting device name to 'RouteQualityTracker Device': "));
  if (! ble.sendCommandCheckOK(F( "AT+GAPDEVNAME=RouteQualityTracker Device" )) ) {
    error(F("Could not set device name?"));
  }

  Serial.println(F("Clearing GATT service and characteristics"));
  if (! ble.sendCommandCheckOK(F("AT+GATTCLEAR"))) {
    error(F("Could not clear GATT service and characteristics"));
  }

  //0x1801

  /* Setup as Generic Attribute Service (0x1801) */
  if (! ble.sendCommandCheckOK(F( "AT+GATTADDSERVICE=UUID=0x1801"  ))) {
    error(F("Could not setup Generic Attribute Service for GATT - 0x1801"));
  }

  // /* Add or remove service requires a reset */
  // Serial.println(F("Performing a SW reset (service changes require a reset): "));
  // if (! ble.reset() ) {
  //   error(F("Couldn't reset??"));
  // }

  Serial.println(F("Adding position characteristic - 0x2A69"));
  if (! ble.sendCommandCheckOK(F( "AT+GATTADDCHAR=UUID=0x2A67,PROPERTIES=0x10,MIN_LEN=1,VALUE=-100" ))) {
    error(F("Couldn't set position characteristic"));
  }

  Serial.println(F("Listing all GATT services and characteristics configuration"));
  ble.println(F( "AT+GATTLIST" ));
}

void waitForButtonPress() {
  Serial.println("waiting for button to be pressed...");
  delay(BUTTON_PRESS_DELAY);
  while (digitalRead(BUTTON_USER) == HIGH) {}
  while (digitalRead(BUTTON_USER) == LOW) {}
}

void setStatusAndWaitForButtonPress(const uint8_t ledPin) {
  while (!ble.isConnected()) {
    Serial.println("Waiting for connection...");

    delay(1000);
    lightLed(ledPin);
    delay(1000);
    lightLed(-1);
  }


  switch (ledPin) {
    case LED_RED:
      Serial.println(F("Setting status to Bad"));
      if (!ble.sendCommandCheckOK(F("AT+GATTCHAR=1,-1"))) {
        error(F("Error sending command"));
        return;
      }
      break;
    case LED_YELLOW:
      Serial.println(F("Setting status to Standard"));
      if (!ble.sendCommandCheckOK(F("AT+GATTCHAR=1,0"))) {
        error(F("Error sending command"));
        return;
      }
      break;
    case LED_GREEN:
      Serial.println(F("Setting status to Good"));
      if (!ble.sendCommandCheckOK(F("AT+GATTCHAR=1,1"))) {
        error(F("Error sending command"));
        return;
      }
      break;
    default:
      Serial.println("unknown status");
      return;
  }

  lightLed(ledPin);
  waitForButtonPress();
}

// the loop function runs over and over again forever
void loop() {

  setStatusAndWaitForButtonPress(LED_YELLOW);
  
  setStatusAndWaitForButtonPress(LED_GREEN);

  setStatusAndWaitForButtonPress(LED_YELLOW);
  
  setStatusAndWaitForButtonPress(LED_RED);
}