class one  {
  public void test1(int a, int b) {
   int c;
   a= 5;   b= 10;   c= a + b;
   return ;
  }
}
final class Main {
  public static void main(String [] args){
    int a=5;
	int b=10;  
one.test1(a,b);
  }
}
