# AI Matching Rules and Implementation

## Matching Criteria

1. **Lifestyle Compatibility (40%)**
   - Matches are scored based on the similarity of lifestyle preferences between users.

2. **Location Proximity (40%)**
   - The geographical distance between users' locations is calculated and factored into the score.

3. **University Affiliation (20%)**
   - Users from the same university are given a higher score.

---

## Key Components

### MatchingController
- **Endpoint**: `GET /api/matching/recommendations`
- **Authorization**: Requires the user to be authenticated and have a premium subscription.
- **Process**:
  - Retrieves the current user's ID from the token.
  - Calls the `IAiMatchingService.GetRoommateRecommendations` method to fetch recommendations.

### IAiMatchingService
- **Method**: `Task<List<RoommateRecommendationDto>> GetRoommateRecommendations(int currentUserId)`
- **Description**: Provides roommate recommendations based on the matching criteria.

### GeoCalculator
- **Purpose**: Calculates the distance between two geographical coordinates.
- **Formula**: Uses the Haversine formula to compute the distance in kilometers.

### AiMatchingService
- **Implementation**:
  - Fetches the current user's profile and university details.
  - Retrieves a list of potential candidates who are verified and have a lifestyle profile.
  - Scores candidates based on:
    - Lifestyle compatibility.
    - Distance to the user's university or residence.
    - University affiliation.
  - Returns a sorted list of `RoommateRecommendationDto`.

### RoommateRecommendationDto
- **Fields**:
  - `UserId`: ID of the recommended user.
  - `FullName`: Name of the recommended user.
  - `AvatarUrl`: Profile picture URL.
  - `UniversityName`: Name of the university.
  - `TotalMatchScore`: Overall compatibility score.
  - `MatchTags`: Tags indicating matching criteria (e.g., "Same University", "Close Location").
  - **Optional**: Information about the user's listing (if they are a host).

---

## Example Workflow

1. **User Request**:
   - A premium user sends a request to `/api/matching/recommendations`.

2. **Service Execution**:
   - The `AiMatchingService` fetches the user's profile and potential candidates.
   - Scores are calculated based on the defined criteria.

3. **Response**:
   - A list of recommended roommates is returned, sorted by compatibility score.

---

For further details, refer to the respective service and controller implementations in the codebase.