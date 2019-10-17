export interface Message { // the order of properties doesn't matter
    id: number;
    senderId: number;
    senderKnownAs: string;  // corresponding to Username property of User class type in Message.cs
    // PhotoUrl Property of User class. The 2 properties simulate the basic functionality of User class in back-end
    senderPhotoUrl: string;
    recipientId: number;
    recipientKnownAs: string;
    recipientPhotoUrl: string;
    content: string;
    isRead: boolean;
    dateRead: Date;
    messageSent: Date;
}
