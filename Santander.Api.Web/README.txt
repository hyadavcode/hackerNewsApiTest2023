ASSUMPTIONS / CONTEXT
==============================================

1. Hacker News API data does not get updated very frequently (i.e. the update latency is not in the order of less than 30 milliseconds
2. The users of this application are not expecting real time live updates and that caching data for a small fraction of time is acceptable here
3. For purpose of this test, no authorization and authentication was required
4. Logging support is implemented right across the code but, no external logger or Telemetry frameworks have been used 


HOW TO RUN
================================================
1. Easiest option is to open in Visual Studio 
2. Set Santander.Api.Web project as Startup project 
3. Run/Debug the application
4. In web browser, navigate to the URL https://localhost:7284/swagger
   This will launch the Swagger UI, which will provide all necessary documentation and tooling on the endpoint 
   and the API endpoint can be invoked directly from there


Low Level Application Architecture and important points
===============================================================================

1. The application is coded in ASP.Net Core 6 / .Net core 6
2. The controllers implement the REST API (in this case there is only 1 Get API for fetching N stories)
3. The application uses CQRS pattern and implements it over MediatR. 
4. There is 1 HttpGet API endpoint HackerNewsFeed/beststories/{storyCount}
5. The solitary API endpoint creates a request as an instance of GetBestStoriesQuery class and calls Send() method on the Mediator to dispatch the query
6. The GetBestStoriesHandler class handles this query. It co-ordinates with the rest of the backend framework and works
    as an orchestrator to get the stories data in the most efficient way
7. DataCache class implements a lightweight in-memory cache which is hosted in-proc. (Ideally this would be replaced by a distributed out-of-proc cache in production environment)
8. HackerNewsService is a proxy to external HackerNews API
9. The duration for which data is cached is configurable is stored in app settings. 
10. The Get API timeout is also controlled via configuration value in app settings.
11. There is configuration of ASP.Net Core Output cache as well although that has not been implemented for this solution.
     Depending on the requirement that can be turned on which can act as first line of cache at the middleware level
13. Response cache is not used here to keep cache control server-side


Application Sequence Flow 
======================================================================================
1.  User  --->   GET API Endpoint  (/hackernewsfeed/beststories/50)

2.  API Endpoint   --> Controller.GetBestStories(50)

3.  Controller   ---> Validates request  / Creates Query  / intializes cancellation token

4.  Controller   ---->  Mediator.Send(query, cancellationToken)

5.       Mediator ---> Handler.Handle(query, cancellationToken)

6.            Handler --> Validates request

7.            Handler --> HackerNewsService.GetBestStoryIds()

8.                  HackerNewsService --> HackerNews API  /v0/beststories.json

9.            Handler --> (on separate background Task) --> DataCache.GetStory()

10.              IF story found in the cache 
                     Then handler uses that object
                 Else Handler --> HackerNewsService.GetStory(storyId) 
                     HackerNewsService ---> Hacker News API  /v0/item/storyId.json
                     Handler --> DataCache.SaveStory()

11. All stories are fetched on separate background Tasks

12. Handler calls Task.WaitAll() to await fetching all the stories either from the cache or from Hacker News API

13. If all tasks complete without error
    Then Handler sorts the data in descending order by score
    And takes the specified number of stories as per the query/request's StoryCount property
    And returns the final result



PROs and CONs of this architecture approach
=============================================================================
PROS ********
1. It leverages a cache between Hacker News API and the Handler which will help in the following ways
     a. Improve overall query performance and latency
     b. Substantially reduce hits to Hacker News API
     c. Cached story objects expriration is controlled via configuration so, it is always possible to 
         tune up or down the throttling behaviour
2. It has plumbing for adding ASP.Net Core Output cache if there is a need to replay the processed data set to 
   all GET requests to the API endpoint within a configurable interval. This would pre-empt not only fetching of 
   storyIds from HackerNews API or fetching of story objects from cache or HackerNews API but, it would also pre-empt
   further sorting of the data

3. In production environment, the in-memory cache can be replaced by distributed out-of-process cache such as Redis which
   can be available over multiple sessions 

4. If the API is deployed in containers then there code would work right out of the box and lean itself to scale out
   scenarious with Docker / Kubernetes cluster management yet, going through a single distributed 

5. Use of HttpClientFactory takes care of socket management effectively

CONS *********
1. This approach might not work entirely on its own for a real time application that deals with over 1000s of transactions
    per second which include all kinds of CRUD operations

2. In such scenario it would require an additional indirection layer of messaging using any standard message broker 
   such as AzureServiceBus over Event Sourcing pattern

3. When the application has started and the first request comes in, it might be slower to process or when the cache expires





****** WHAT else could be done in a proper production application or there was more time to implement a better solution ***
=======================================================================================================================

1. Add more unit test coverage. Currently unit tests are done selectively for Controller and Handler only
2. Add performance tests
3. Use Message Queue in addition to the in-memory cache and use Event Sourcing pattern




