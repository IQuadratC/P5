#define STP_1 54
#define DIR_1 55
#define ENA_1 38

#define STP_2 60
#define DIR_2 61
#define ENA_2 56

#define STP_3 46
#define DIR_3 48
#define ENA_3 62

#define STP_4 26
#define DIR_4 28
#define ENA_4 24

#define LED_PIN 13

int Steps_Motor_1 = -1;
int Steps_Motor_2 = -1;
int Steps_Motor_3 = -1;
int Steps_Motor_4 = -1;

int Delay_Motor_1;
int Delay_Motor_2;
int Delay_Motor_3;
int Delay_Motor_4;

#include <Wire.h>

void setup() {
  Wire.begin(8);                // join i2c bus with address #8
  Wire.onReceive(receiveEvent); // register event
  Serial.begin(9600);           // start serial for output

  TCCR0A = (1 << WGM01); //Set the CTC mode
  OCR0A = 15;

  TIMSK0 |= (1 << OCIE0A); //Set the interrupt request
  sei(); //Enable interrupt
  TCCR0B = 0;
  TCCR0B |= (1 << CS00);
  TCCR0B |= (1 << CS01);

  // Stepper pins foreach Motor
  pinMode(STP_1, OUTPUT);
  pinMode(DIR_1, OUTPUT);
  pinMode(ENA_1, OUTPUT);


  pinMode(STP_2, OUTPUT);
  pinMode(DIR_2, OUTPUT);
  pinMode(ENA_2, OUTPUT);

  pinMode(STP_3, OUTPUT);
  pinMode(DIR_3, OUTPUT);
  pinMode(ENA_3, OUTPUT);

  pinMode(STP_4, OUTPUT);
  pinMode(DIR_4, OUTPUT);
  pinMode(ENA_4, OUTPUT);

  pinMode(LED_PIN, OUTPUT);
  //set enabled pins to low (enabled pin is inverted)
  digitalWrite(ENA_1, LOW);
  digitalWrite(ENA_2, LOW);
  digitalWrite(ENA_3, LOW);
  digitalWrite(ENA_4, LOW);
  digitalWrite(LED_PIN, HIGH);
//set default value for direction
  digitalWrite(DIR_1, HIGH);
  digitalWrite(DIR_2, HIGH);
  digitalWrite(DIR_3, HIGH);
  digitalWrite(DIR_4, HIGH);

  Delay_Motor_1 = -1;
  Delay_Motor_2 = -1;
  Delay_Motor_3 = -1;
  Delay_Motor_4 = -1;
  
}

ISR(TIMER0_COMPA_vect){
  //Stepper make on Stepp 
  if (Steps_Motor_1 != 255) {
    if (Steps_Motor_1-- == 0) {
      digitalWrite(STP_1, HIGH);
      digitalWrite(STP_1, LOW);
      Steps_Motor_1 = Delay_Motor_1;
    }
  }else{
    Steps_Motor_1 = Delay_Motor_1;
  }

  if (Steps_Motor_2 != 255) {
    if (Steps_Motor_2-- == 0) {
      digitalWrite(STP_2, HIGH);
      digitalWrite(STP_2, LOW);
      Steps_Motor_2 = Delay_Motor_2;
    }
  }
  else{
    Steps_Motor_2 = Delay_Motor_2;
  }
  if (Steps_Motor_3 != 255) {
    if (Steps_Motor_3-- == 0) {
      digitalWrite(STP_3, HIGH);
      digitalWrite(STP_3, LOW);
      Steps_Motor_3 = Delay_Motor_3;
    }
  }
  else{
    Steps_Motor_3 = Delay_Motor_3;
  }
  if (Steps_Motor_4 != 255) {
    if (Steps_Motor_4-- == 0) {
      digitalWrite(STP_4, HIGH);
      digitalWrite(STP_4, LOW);
      Steps_Motor_4 = Delay_Motor_4;
    }
  }
  else{
    Steps_Motor_4 = Delay_Motor_4;
  }
}
void loop() {

}

// function that executes whenever data is received from master
// this function is registered as an event, see setup()
void receiveEvent(int howMany) {
  while (0 < Wire.available()) { // loop through all
    char data = Wire.read(); // receive byte as a character
    if (data == 'M') {
      char WitchMotor = Wire.read();
      if (WitchMotor == '1') {
        char Motor1_Status = Wire.read();
        if (Motor1_Status == 'S') {
          byte Motor1_Speed = Wire.read(); 
          Delay_Motor_1 = (int)Motor1_Speed;
        } else if (Motor1_Status == 'D') {
          byte Motor2_Diraction = Wire.read();
          if (Motor2_Diraction == 1) {
            digitalWrite(DIR_1, HIGH);
          } else if (Motor2_Diraction == 2) {
            digitalWrite(DIR_1, LOW);
          }
        }
      }
      if (WitchMotor == '2') {
        char Motor2_Status = Wire.read();
        if (Motor2_Status == 'S') {
          byte Motor2_Speed = Wire.read();
          Delay_Motor_2 = Motor2_Speed;
        } else if (Motor2_Status == 'D') {
          byte Motor2_Diraction = Wire.read();
          if (Motor2_Diraction == 1) {
            digitalWrite(DIR_2, HIGH);
          } else if (Motor2_Diraction == 2) {
            digitalWrite(DIR_2, LOW);
          }
        }
      }
      if (WitchMotor == '3') {
        char Motor3_Status = Wire.read();
        if (Motor3_Status == 'S') {
          byte Motor3_Speed = Wire.read();
          Delay_Motor_3 = Motor3_Speed;
        } else if (Motor3_Status == 'D') {
          byte Motor3_Diraction = Wire.read();
          if (Motor3_Diraction == 1) {
            digitalWrite(DIR_3, HIGH);
          } else if (Motor3_Diraction == 2) {
            digitalWrite(DIR_3, LOW);
          }
        }
      }
      if (WitchMotor == '4') {
        char Motor4_Status = Wire.read();
        if (Motor4_Status == 'S') {
          byte Motor4_Speed = Wire.read();
          Serial.println(Motor4_Speed);
          Delay_Motor_4 = Motor4_Speed;
        } else if (Motor4_Status == 'D') {
          byte Motor4_Diraction = Wire.read();
          if (Motor4_Diraction == 1) {
            digitalWrite(DIR_4, HIGH);
          } else if (Motor4_Diraction == 2) {
            digitalWrite(DIR_4, LOW);
          }
        }
      }
        
    }
  }
}
