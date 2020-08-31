class classone{
  public void firstclass(){
    return;
  }
  public int secondclass(){
    int a,b,c,d;

    write("Enter a number",a,b);
    read(a,b,c);
    b=10;
    d=20;
    c=d-a*b;
    write("The answer is ");
    writeln(a,b,c,d);
    return c;
  }
}
final class Main{
  public static void main(String [] args){
    classone.secondclass();
  }
}
