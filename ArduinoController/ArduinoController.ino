#define ON    0
#define OFF   1

#define FRONT 0
#define BACK  1
#define RIGHT 2
#define LEFT  3
#define DUMMY 5

const int peltier[4][2] = {{6,7},{8,9},{10,11},{12,13}};
char input;
String command  = "";
char macroCommand;
char microCommand;
char dummyCommand;
bool isCommandComplete = false;

void setRelayState(unsigned char id, unsigned char state1, unsigned char state2){
  digitalWrite(peltier[id][0], state1);
  digitalWrite(peltier[id][1], state2);
}

void getCommand(){
    while(Serial.available() && !isCommandComplete){
        input = Serial.read();
        command += input;
        if(input == '\n'){
            isCommandComplete = true;
        }
    }
}

void heatPeltier(unsigned char id, unsigned char dummyCommand = 'o'){
  setRelayState(id, ON, ON);  
  Serial.print(id);
  Serial.println(" is on");
  if(dummyCommand == 'o'){      
    digitalWrite(DUMMY, OFF);
    Serial.println("5 is off");
  }
  else if(dummyCommand == 'n'){
    digitalWrite(DUMMY, ON);
    Serial.println("5 is on");
  }
}

void disablePeltier(unsigned char id, unsigned char dummyCommand = 'o'){
  setRelayState(id, ON, OFF);    
  Serial.print(id);
  Serial.println(" is off");
  if(dummyCommand == 'o'){      
    digitalWrite(DUMMY, OFF);
    Serial.println("5 is off");
  }
  else if(dummyCommand == 'n'){
    digitalWrite(DUMMY, ON);
    Serial.println("5 is on");
  }
}

void coolPeltier(unsigned char id, unsigned char dummyCommand = 'o'){
  setRelayState(id, OFF, OFF);
  Serial.print(id);
  Serial.println(" is reversed");
  if(dummyCommand == 'o'){      
    digitalWrite(DUMMY, OFF);
    Serial.println("5 is off");
  }
  else if(dummyCommand == 'n'){
    digitalWrite(DUMMY, ON);
    Serial.println("5 is on");
  }
}

void initRelay(void){
  for(int i = 0; i < 4; i++){
    pinMode(peltier[i][0], OUTPUT);
    pinMode(peltier[i][1], OUTPUT);
    disablePeltier(i, '-');
  }
  pinMode(DUMMY, OUTPUT);
  digitalWrite(DUMMY, OFF);
}

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);//(115200);
  initRelay();
}

void loop() {
  // put your main code here, to run repeatedly:
  if(isCommandComplete){
    macroCommand = command.charAt(0);
    microCommand = command.charAt(1);
    dummyCommand = command.charAt(2);

    command = "";
    isCommandComplete = false;
    
    switch(macroCommand){
        case 'F':
          if(microCommand == 'n'){
              heatPeltier(FRONT, dummyCommand);
          }
          else if(microCommand == 'r'){
              coolPeltier(FRONT, dummyCommand);
          }
          else{
              disablePeltier(FRONT, dummyCommand);
          }
          break;
        case 'B':
          if(microCommand == 'n'){
              heatPeltier(BACK, dummyCommand);
          }
          else if(microCommand == 'r'){
              coolPeltier(BACK, dummyCommand);
          }
          else{
              disablePeltier(BACK, dummyCommand);
          }
          break;
        case 'R':
          if(microCommand == 'n'){
              heatPeltier(RIGHT, dummyCommand);
          }
          else if(microCommand == 'r'){
              coolPeltier(RIGHT, dummyCommand);
          }
          else{
              disablePeltier(RIGHT, dummyCommand);
          }
          break;
        case 'L':
          if(microCommand == 'n'){
              heatPeltier(LEFT, dummyCommand);
          }
          else if(microCommand == 'r'){
              coolPeltier(LEFT, dummyCommand);
          }
          else{
              disablePeltier(LEFT, dummyCommand);
          }
          break;
        case 'o':
          disablePeltier(FRONT, "o");
          disablePeltier(BACK, "-");
          disablePeltier(RIGHT, "-");
          disablePeltier(LEFT, "-");
          break;
        default:
          Serial.println("no usable macroCommand");
          break;
    }
    
  }else{
    getCommand();
  }
}