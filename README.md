[![Build status](https://ci.appveyor.com/api/projects/status/bjyiix8lvlygpu4s/branch/master?svg=true)](https://ci.appveyor.com/project/adrianiftode/circuitbreaker/branch/master)

# CircuitBreaker
An implementation of the CircuitBreaker pattern

##### just for fun


## Sample usage

  ```
  var circuitBreaker = new CircuitBreaker();
 
  using (var scope = circuitBreaker.GetScope())
  {
      service.GetSomeDataFromNetwork();
  }
  ```
