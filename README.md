[![Build status](https://ci.appveyor.com/api/projects/status/bjyiix8lvlygpu4s/branch/master?svg=true)](https://ci.appveyor.com/project/adrianiftode/circuitbreaker/branch/master)

# CircuitBreaker
An implementation of the CircuitBreaker pattern.
For proper handling of resilience and transient issues please check [Polly](https://github.com/App-vNext/Polly)

##### just for fun


## Sample usage

  ```csharp
  var circuitBreaker = new CircuitBreaker();
 
  using (var scope = circuitBreaker.GetScope())
  {
      service.GetSomeDataFromNetwork();
  }
  ```