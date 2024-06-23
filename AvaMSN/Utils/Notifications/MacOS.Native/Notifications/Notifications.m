//
//  Notifications.m
//  Notifications
//
//  Created by campos on 6/20/24.
//

#import <UserNotifications/UserNotifications.h>

void requestAuthorization(void) {
    @autoreleasepool {
        UNUserNotificationCenter* center = [UNUserNotificationCenter currentNotificationCenter];
        UNAuthorizationOptions options = UNAuthorizationOptionAlert;
        
        [center requestAuthorizationWithOptions:options
         completionHandler:^(BOOL granted, NSError * _Nullable error) {
            if (!granted)
                NSLog(@"%@", error);
        }];
    }
}

void showNotification(char* title, char* body) {
    @autoreleasepool {
        UNUserNotificationCenter* center = [UNUserNotificationCenter currentNotificationCenter];
        
        UNMutableNotificationContent* content = [[UNMutableNotificationContent alloc] init];
        content.title = [NSString stringWithUTF8String:title];
        content.body = [NSString stringWithUTF8String:body];

        UNTimeIntervalNotificationTrigger *trigger = [UNTimeIntervalNotificationTrigger
            triggerWithTimeInterval:1 repeats: NO];
        
        UNNotificationRequest *request = [UNNotificationRequest requestWithIdentifier:@"rid" content:content trigger:trigger];
        [center addNotificationRequest:request withCompletionHandler:^(NSError * _Nullable error) {
            if (error)
                NSLog(@"%@", error);
        }];
    }
}
