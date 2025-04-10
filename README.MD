# NakliyeProjesi API Documentation

## Overview

The NakliyeProjesi API provides functionality for managing shipments, bids, user profiles, ratings, notifications, and authentication. It also supports real-time notifications using SignalR.

---

## Table of Contents

1. [Authentication](#authentication)
2. [Shipments](#shipments)
3. [Bids](#bids)
4. [Ratings](#ratings)
5. [Notifications](#notifications)
6. [Profiles](#profiles)
7. [Real-Time Notifications (SignalR)](#real-time-notifications-signalr)
8. [Payment (Placeholder)](#payment-placeholder)
9. [Notes](#notes)

---

## Authentication

### Register a New User
**POST** `/api/auth/register`

**Request Body**:
```json
{
  "fullName": "John Doe",
  "email": "johndoe@example.com",
  "password": "password123",
  "phoneNumber": "1234567890",
  "userType": 1
}
```

**Response**:
- `200 OK`: Registration successful.
- `400 Bad Request`: Validation errors.

---

### Login
**POST** `/api/auth/login`

**Request Body**:
```json
{
  "email": "johndoe@example.com",
  "password": "password123"
}
```

**Response**:
- `200 OK`: Returns a JWT token.
- `401 Unauthorized`: Invalid credentials.

---

### Verify Email
**GET** `/api/auth/verify-email?token={token}`

**Response**:
- `200 OK`: Email verified successfully.
- `400 Bad Request`: Invalid or expired token.

---

### Forgot Password
**POST** `/api/auth/forgot-password`

**Request Body**:
```json
{
  "email": "johndoe@example.com"
}
```

**Response**:
- `200 OK`: Password reset link sent.

---

### Reset Password
**POST** `/api/auth/reset-password`

**Request Body**:
```json
{
  "token": "reset-token",
  "newPassword": "newpassword123"
}
```

**Response**:
- `200 OK`: Password reset successfully.
- `400 Bad Request`: Invalid or expired token.

---

## Shipments

### Get All Shipments
**GET** `/api/shipments`

**Response**:
- `200 OK`: Returns a list of shipments.

---

### Get Shipment by ID
**GET** `/api/shipments/{id}`

**Response**:
- `200 OK`: Returns shipment details.
- `404 Not Found`: Shipment not found.

---

### Create a Shipment
**POST** `/api/shipments`

**Request Body**:
```json
{
  "title": "Shipment Title",
  "description": "Shipment Description",
  "pickupLocation": "Location A",
  "deliveryLocation": "Location B",
  "pickupDate": "2023-12-01T10:00:00Z"
}
```

**Response**:
- `201 Created`: Shipment created successfully.
- `403 Forbidden`: Only customers can create shipments.

---

### Search Shipments
**GET** `/api/shipments/search?title={title}&location={location}&status={status}`

**Response**:
- `200 OK`: Returns filtered shipments.

---

### Upload Shipment Photo
**POST** `/api/shipments/{id}/upload-photo`

**Form Data**:
- `photo`: File to upload.

**Response**:
- `200 OK`: Photo uploaded successfully.
- `404 Not Found`: Shipment not found.

---

## Bids

### Get Bids by User
**GET** `/api/bid/by-user`

**Response**:
- `200 OK`: Returns bids placed by the authenticated user.

---

### Create a Bid
**POST** `/api/bid`

**Request Body**:
```json
{
  "price": 100.50,
  "shipmentId": 1
}
```

**Response**:
- `200 OK`: Bid created successfully.
- `400 Bad Request`: Validation errors or duplicate bid.

---

### Delete a Bid
**DELETE** `/api/bid/{id}`

**Response**:
- `204 No Content`: Bid deleted successfully.
- `403 Forbidden`: User not authorized to delete this bid.

---

## Ratings

### Add a Rating
**POST** `/api/rating`

**Request Body**:
```json
{
  "userId": 2,
  "shipmentId": 1,
  "score": 5,
  "comment": "Great service!"
}
```

**Response**:
- `200 OK`: Rating added successfully.
- `400 Bad Request`: Duplicate rating for the same shipment.

---

### Get Ratings for a User
**GET** `/api/rating/user/{userId}`

**Response**:
- `200 OK`: Returns ratings for the specified user.

---

## Notifications

### Get Notifications
**GET** `/api/notification`

**Response**:
- `200 OK`: Returns notifications for the authenticated user.

---

### Mark Notification as Read
**PUT** `/api/notification/mark-as-read/{id}`

**Response**:
- `200 OK`: Notification marked as read.
- `404 Not Found`: Notification not found.

---

### Send Notification
**POST** `/api/notification/send`

**Request Body**:
```json
{
  "userId": 1,
  "message": "Your shipment has been completed."
}
```

**Response**:
- `200 OK`: Notification sent successfully.
- `404 Not Found`: User not found.

---

## Profiles

### Get Profile
**GET** `/api/profile`

**Response**:
- `200 OK`: Returns the authenticated user's profile.

---

### Update Profile
**PUT** `/api/profile`

**Request Body**:
```json
{
  "fullName": "John Doe",
  "phoneNumber": "1234567890"
}
```

**Response**:
- `200 OK`: Profile updated successfully.

---

### Change Password
**PUT** `/api/profile/change-password`

**Request Body**:
```json
{
  "currentPassword": "oldpassword123",
  "newPassword": "newpassword123"
}
```

**Response**:
- `200 OK`: Password changed successfully.
- `400 Bad Request`: Invalid current password.

---

### Upload Profile Photo
**POST** `/api/profile/upload-photo`

**Form Data**:
- `photo`: File to upload.

**Response**:
- `200 OK`: Photo uploaded successfully.

---

## Real-Time Notifications (SignalR)

### SignalR Hub
**Endpoint**: `/notificationHub`

### SignalR Client Setup
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

connection.on("ReceiveNotification", (message) => {
    console.log("New notification:", message);
    alert(`New notification: ${message}`);
});

connection.start()
    .then(() => console.log("SignalR connected"))
    .catch(err => console.error("SignalR connection error:", err));
```

---

## Payment (Placeholder)

### Process Payment
**POST** `/api/payment`

**Request Body**:
```json
{
  "amount": 100.50,
  "currency": "USD",
  "description": "Payment for shipment"
}
```

**Response**:
- `200 OK`: Payment processed successfully (placeholder response).

---

## Notes

- All endpoints requiring authentication must include a valid JWT token in the `Authorization` header:  
  `Authorization: Bearer {token}`

- Ensure proper validation for all request payloads to avoid errors.

- For file uploads, use `multipart/form-data` content type.

---

## Future Enhancements

- Integration with a payment provider.
- Advanced analytics and reporting.
